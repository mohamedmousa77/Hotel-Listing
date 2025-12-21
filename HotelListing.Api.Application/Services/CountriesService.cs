using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Country;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Application.Services;

public class CountriesService(HotelListingDbContext context, IMapper mapper) : ICountriesService
{
    public async Task<Result<IEnumerable<ReadCountriesDto>>> GetCountriesAsync()
    {
        var countries = await context.Countries
            .ProjectTo<ReadCountriesDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<IEnumerable<ReadCountriesDto>>.Success(countries);
    }

    public async Task<Result<ReadCountryDto>> GetCountryAsync(int id)
    {

        var country = await context.Countries
            .Where(c => c.CountryId == id)
            .ProjectTo<ReadCountryDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return country is null
            ? Result<ReadCountryDto>.Failure(new Error(ErrorCodes.NotFound, $"Country {id} was not found. "))
            : Result<ReadCountryDto>.Success(country);
    }

    public async Task<Result> UpdateCountryAsync(int id, [FromBody] UpdateCountryDto countryDto)
    {
        try
        {
            if (id != countryDto.Id)
            {
                return Result.BadRequest(new Error(ErrorCodes.Validation, "Id route value does not match payload Id. "));
            }

            var country = await context.Countries.FindAsync(id);
            if (country is null)
            {
                return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country with ID {id} not found."));
            }

            bool duplicateName = await CountryExistsAsync(countryDto.Name);

            if (duplicateName)
            {
                return Result.Failure(
                    new Error(ErrorCodes.Conflict, $"Country with the name {countryDto.Name} already exists"));
            }

            mapper.Map(countryDto, country);
            await context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error(ErrorCodes.Failure, "An unexpected errror occurred while updating the country."));
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
                    .Failure(new Error(ErrorCodes.Conflict, $"Country with the name: {countryDto.Name} already exists"));
            }

            var country = mapper.Map<Country>(countryDto);
            context.Countries.Add(country);
            await context.SaveChangesAsync();

            var resultDto = await context.Countries
                .Where(c => c.CountryId == country.CountryId)
                .ProjectTo<ReadCountryDto>(mapper.ConfigurationProvider)
                .FirstAsync();


            return Result<ReadCountryDto>.Success(resultDto);
        }
        catch (Exception)
        {
            return Result<ReadCountryDto>.Failure(new Error(ErrorCodes.Failure, "An unexpected errror occurred while creating the country."));
        }

    }
    public async Task<Result> DeleteCountryAsync(int id)
    {
        try
        {
            var country = await context.Countries.FindAsync(id);
            if (country is null)
            {
                return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country with ID {id} was not found."));
            }

            context.Countries.Remove(country);
            await context.SaveChangesAsync();

            return Result.Success();

        }
        catch (Exception)
        {
            return Result.Failure(new Error(ErrorCodes.Failure, "An unexpected errror occurred while deleting the country."));
        }

    }


    public async Task<bool> CountryExistsAsync(int id) => context.Countries.Any(e => e.CountryId == id);
    public async Task<bool> CountryExistsAsync(string name)
        => await context.Countries.
        AnyAsync(e => e.Name.ToLower().Trim() == name.ToLower().Trim());

}
