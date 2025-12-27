using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Hotel;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Extentions;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Application.Services;

public class HotelsService(
    HotelListingDbContext context,
    IMapper mapper,
    ICountriesService countriesService
    ) : IHotelsService
{
    public async Task<Result<PagedResult<GetHotelDto>>> GetHotelsAsync(
        PaginationParameters paginationParameters, HotelFilterParameters filters)
    {
        // This query now is looking to the entire table but will be built upon based on the filters provided
        var query = context.Hotels.AsQueryable();

        if ( filters.CountryId.HasValue)        
            query = query.Where(h => h.CountryId == filters.CountryId);
        
        if (filters.MinimumRating.HasValue)        
            query = query.Where(h => h.Rating >= filters.MinimumRating);
        
        if (filters.MaximumRating.HasValue)        
            query = query.Where(h => h.Rating <= filters.MaximumRating);
        
        if (filters.MinPrice.HasValue)        
            query = query.Where(h => h.PerNightRate >= filters.MinPrice);
        
        if (filters.MaxPrice.HasValue)        
            query = query.Where(h => h.PerNightRate <= filters.MaxPrice);
        
        if (!string.IsNullOrWhiteSpace(filters.Location))
        {
            var location = filters.Location.Trim().ToLower();
            query = query.Where(h => h.Address != null && EF.Functions.Like(h.Address, $"%{location}%") );

        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var genericSearch = filters.Search.Trim().ToLower();
            query = query.Where(h => EF.Functions.Like(h.Name, $"%{genericSearch}%") ||
                                    EF.Functions.Like(h.Address, $"%{genericSearch}%") );
        }

        query = filters.SortBy?.ToLower() switch
        {            
            "name" => filters.IsSortDescending ? query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name),
            "rating" => filters.IsSortDescending ? query.OrderByDescending(h => h.Rating) : query.OrderBy(h => h.Rating),
            "address" => filters.IsSortDescending ? query.OrderByDescending(h => h.Address) : query.OrderBy(h => h.Address),
            _ => query.OrderBy(h => h.Name)
        };

        var hotels =await query
            .Include(c => c.Country)
            .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
            .ToPageResultAsync(paginationParameters);

        return Result<PagedResult<GetHotelDto>>.Success(hotels);
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


        var resultObj = mapper.Map<GetHotelDto>(hotel);


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
        context.Entry(hotel).State = EntityState.Modified;
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
    {
        var normalizedName = name.ToLower().Trim();
        return await context.Hotels.AnyAsync(e => e.Name.ToLower().Trim() == normalizedName && 
                                                e.CountryId == countryId);
    }
}
