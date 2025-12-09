using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Hotel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

public class HotelsService(HotelListingDbContext context) : IHotelsService
{
    public async Task<IEnumerable<GetHotelDto>> GetHotelsAsync()
    {
        // SELECT * FROM HOTEL LEFT JOIN Countries ON Hotels.CountryId = Countries.countryId
        return await context.Hotels
            .Include(c => c.Country)
             .Select(h => new GetHotelDto(h.Id, h.Name, h.Address, h.Rating, h.Country!.Name, h.CountryId))
             .ToListAsync();
    }

    public async Task<GetHotelDto> GetHotelAsync(int id)
    {

        var hotel = await context.Hotels
            .Where(h => h.Id == id)
            .Select(h => new GetHotelDto(
                h.Id,
                h.Name,
                h.Address,
                h.Rating,
                h.Country!.Name,
                h.CountryId
            )).FirstOrDefaultAsync();
        return hotel ?? throw new KeyNotFoundException($"Hotel with ID {id} not found. ");
    }

    public async Task UpdateHotelAsync(int id, [FromBody] UpdateHotelDto hotelDto)
    {
        var hotel = await context.Hotels.FindAsync(id)
            ?? throw new KeyNotFoundException($"Hotel with ID {id} not found. ");

        hotel.Name = hotelDto.Name;
        hotel.Address = hotelDto.Address;
        hotel.Rating = hotelDto.Rating;
        hotel.CountryId = hotelDto.CountryId;

        context.Entry(hotel).State = EntityState.Modified;
        context.Hotels.Update(hotel);
        await context.SaveChangesAsync();
    }

    public async Task DeleteHotelAsync(int id)
    {


        await context.Hotels
            .Where(h => h.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<GetHotelDto> CreateHotelAsync(CreateHotelDto hotelDto)
    {
        var hotel = new Hotel
        {
            Name = hotelDto.Name,
            Address = hotelDto.Address,
            Rating = hotelDto.Rating,
            CountryId = hotelDto.CountryId,
        };

        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        return new GetHotelDto (
           hotel.Id,
            hotel.Name,
            hotel.Address,
            hotel.Rating,
            string.Empty,
            hotel.CountryId
            );
    }
    public async Task<bool> HotelExistsAsync(int id) => context.Hotels.Any(e => e.Id == id);
    public async Task<bool> HotelExistsAsync(string name) => context.Hotels.Any(e => e.Name == name);

}
