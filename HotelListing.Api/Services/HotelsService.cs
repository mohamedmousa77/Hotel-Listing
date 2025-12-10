using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Hotel;
using HotelListing.Api.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

public class HotelsService(
    HotelListingDbContext context,
    IMapper mapper,
    ICountriesService countriesService
    ) : IHotelsService
{
    public async Task<Result<IEnumerable<GetHotelDto>>> GetHotelsAsync()
    {
        var hotels = await context.Hotels
            .Include(c => c.Country)
            .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
             .ToListAsync();

        return Result<IEnumerable<GetHotelDto>>.Success(hotels);
    }

    public async Task<Result<GetHotelDto>> GetHotelAsync(int id)
    {
        var hotel = await context.Hotels
            .Where(h => h.Id == id)
            .Include(q => q.Country)
            .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (hotel == null)
        {
            return Result<GetHotelDto>.Failure(new Error(ErrorCodes.NotFound, $"Hotel with ID {id} not found. "));
        }

        return Result<GetHotelDto>.Success(hotel);
    }

    public async Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto)
    {
        bool countryExists = await countriesService.CountryExistsAsync(hotelDto.CountryId);
        if (countryExists)
        {
            return Result<GetHotelDto>.Failure(new Error(ErrorCodes.NotFound, $"country {hotelDto.CountryId} was not found. "));
        }

        bool duplicatedHotelName = await HotelExistsAsync(hotelDto.Name, hotelDto.CountryId);
        if (duplicatedHotelName)
        {
            return Result<GetHotelDto>.Failure(new Error(ErrorCodes.Conflict, $"Hotel name {hotelDto.Name} already exists. "));

        }

        var hotel = mapper.Map<Hotel>(hotelDto);
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();


        var resultObj = await context.Hotels
            .Where(h => h.Id == hotel.Id)
            .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
            .FirstAsync();


        return Result<GetHotelDto>.Success(resultObj);

    }

    public async Task<Result> UpdateHotelAsync(int id, [FromBody] UpdateHotelDto hotelDto)
    {
        if (hotelDto.Id != id)
        {
            Result.BadRequest(new Error(ErrorCodes.Validation, "ID route value does not match payload ID. "));
        }


        var hotel = await context.Hotels.FindAsync(id);
        if (hotel == null)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Hotel with ID {id} was not found. "));
        }

        var countryExists = await countriesService.CountryExistsAsync(hotelDto.CountryId);
        if (!countryExists)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country with ID {hotelDto.CountryId} was not found. "));
        }

        mapper.Map(hotelDto, hotel);
        context.Hotels.Update(hotel);
        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteHotelAsync(int id)
    {
        var effected = await context.Hotels
            .Where(h => h.Id == id)
            .ExecuteDeleteAsync();

        if (effected == 0)
        {
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Hotel with ID {id} was not found. "));
        }

        return Result.Success();
    }


    public async Task<bool> HotelExistsAsync(int id) => await context.Hotels.AnyAsync(e => e.Id == id);
    public async Task<bool> HotelExistsAsync(string name, int countryId)
        => await context.Hotels.AnyAsync(e => e.Name.ToLower().Trim() == name.ToLower().Trim() && e.CountryId == countryId);

}
