using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Country;
using HotelListing.Api.DTOs.Hotel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

public class CountriesService(HotelListingDbContext context) : ICountriesService
{
    public async Task<IEnumerable<ReadCountriesDto>> GetCountriesAsync()
    {
        return await context.Countries
            .Select(c => new ReadCountriesDto(
                c.CountryId,
                c.Name,
                c.ShortName
            )).ToListAsync();
    }

    public async Task<ReadCountryDto> GetCountryAsync(int id)
    {

        var country = await context.Countries
            .Where(c => c.CountryId == id)
            .Select(c => new ReadCountryDto(
                c.CountryId,
                c.Name,
                c.ShortName,
                c.Hotels.Select(h => new GetHotelSlimDto(
                    h.Id,
                    h.Name,
                    h.Address,
                    h.Rating
                )).ToList()
            )).FirstOrDefaultAsync();
        return country ?? throw new KeyNotFoundException($"Country with ID {id} not found. ");
    }

    public async Task UpdateCountryAsync(int id, [FromBody] UpdateCountryDto countryDto)
    {
        var country = await context.Countries.FindAsync(id) 
            ?? throw new KeyNotFoundException($"Country with ID {id} not found. ");
        
        country.Name = countryDto.Name;
        country.ShortName = countryDto.ShortName;

        context.Entry(country).State = EntityState.Modified;
        context.Countries.Update(country);
        await context.SaveChangesAsync();
    }

    public async Task DeleteCountryAsync(int id)
    {
        var country = await context.Countries.FindAsync(id)
            ?? throw new KeyNotFoundException($"Country with ID {id} not found. ");

        context.Countries.Remove(country);
        await context.SaveChangesAsync();
    }

    public async Task<ReadCountryDto> CreateCountryAsync(CreateCountryDto countryDto)
    {
        var country = new Country
        {
            Name = countryDto.Name,
            ShortName = countryDto.ShortName
        };

        context.Countries.Add(country);
        await context.SaveChangesAsync();

        return new ReadCountryDto(
            country.CountryId, 
            country.Name, 
            country.ShortName, 
            []
        );
    }
    public async Task<bool> CountryExistsAsync(int id) => context.Countries.Any(e => e.CountryId == id);
    public async Task<bool> CountryExistsAsync(string name) => context.Countries.Any(e => e.Name == name);

}
