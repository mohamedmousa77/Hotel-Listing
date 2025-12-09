using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Country;
using HotelListing.Api.DTOs.Hotel;
using HotelListing.Api.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

public class CountriesService(HotelListingDbContext context) : ICountriesService
{
    public async Task<Result<IEnumerable<ReadCountriesDto>>> GetCountriesAsync()
    {
        var countries = await context.Countries
            .Select(c => new ReadCountriesDto(
                c.CountryId,
                c.Name,
                c.ShortName
            )).ToListAsync();

        return Result<IEnumerable<ReadCountriesDto>>.Success(countries);
    }

    public async Task<Result<ReadCountryDto>> GetCountryAsync(int id)
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

        return country is null
            ? Result<ReadCountryDto>.NotFound()
            : Result<ReadCountryDto>.Success(country);
    }

    public async Task<Result> UpdateCountryAsync(int id, [FromBody] UpdateCountryDto countryDto)
    {
        try
        {
            if (id != countryDto.Id)
            {
                return Result.BadRequest(new Error("Validation", "Id route value does not match payload Id. "));
            }

            var country = await context.Countries.FindAsync(id);
            if (country is null)
            {
                return Result.NotFound(new Error("Validation", $"Country with ID {id} not found."));
            }

            bool duplicateName = await CountryExistsAsync(countryDto.Name);

            if (duplicateName)
            {
                return Result.Failure(
                    new Error("Conflict", $"Country with the name {countryDto.Name} already exists"));
            }

            country.Name = countryDto.Name;
            country.ShortName = countryDto.ShortName;

            context.Entry(country).State = EntityState.Modified;
            context.Countries.Update(country);
            await context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure();
        }
    }

    public async Task<Result> DeleteCountryAsync(int id)
    {
        try
        {
            var country = await context.Countries.FindAsync(id);
            if (country is null)
            {
                return Result.NotFound(new Error("Validation", $"Country with ID {id} not found."));
            }

            context.Countries.Remove(country);
            await context.SaveChangesAsync();

            return Result.Success();

        }
        catch (Exception)
        {
            return Result.Failure();
        }

    }

    public async Task<Result<ReadCountryDto>> CreateCountryAsync(CreateCountryDto countryDto)
    {
        try
        {

            var exists = await CountryExistsAsync(countryDto.Name);
            if (exists)
            {
                return Result<ReadCountryDto>
                    .Failure(new Error("Conflict", $"Country with the name: {countryDto.Name} already exists"));
            }

            var country = new Country
            {
                Name = countryDto.Name,
                ShortName = countryDto.ShortName
            };

            context.Countries.Add(country);
            await context.SaveChangesAsync();

            var resultDto = new ReadCountryDto(
                country.CountryId,
                country.Name,
                country.ShortName,
                []
            );
            return Result<ReadCountryDto>.Success(resultDto);
        }
        catch (Exception)
        {
            return Result<ReadCountryDto>.Failure();
        }

    }
    public async Task<bool> CountryExistsAsync(int id) => context.Countries.Any(e => e.CountryId == id);
    public async Task<bool> CountryExistsAsync(string name)
        => await context.Countries.
        AnyAsync(e => e.Name.ToLower().Trim() == name.ToLower().Trim());

}
