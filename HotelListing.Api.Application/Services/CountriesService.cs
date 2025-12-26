using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Country;
using HotelListing.Api.Application.DTOs.Hotel;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Extentions;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Application.Services;

public class CountriesService(HotelListingDbContext context, IMapper mapper) : ICountriesService
{
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync(CountryFilterParameters filters)
    {
        var query = context.Countries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
           var term = filters.Search.Trim();
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{term}%")
            || EF.Functions.Like(c.ShortName, $"%{term}%"));
        }
        var countries = await query
            .AsNoTracking()
            .ProjectTo<GetCountriesDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<IEnumerable<GetCountriesDto>>.Success(countries);
    }
    public async Task<Result<GetCountryHotelsDto>> GetCountryHotelsAsync(
        int countryId, PaginationParameters paginationParameters, CountryFilterParameters filters)
    {
        var exists = await CountryExistsAsync(countryId);
        if (!exists)
        {
            return Result<GetCountryHotelsDto>.Failure(
                new Error(ErrorCodes.NotFound, $"Country with ID {countryId} was not found."));
        }

        var countryName = await context.Countries
            .Where(c => c.CountryId == countryId)
            .Select(q => q.Name)
            .SingleAsync();

        var hotelQuery = context.Hotels
            .Where(h => h.CountryId == countryId)
            .AsQueryable();

        if(!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            hotelQuery = hotelQuery.Where(h => EF.Functions.Like(h.Name, $"%{term}%"));
        }

        hotelQuery = (filters.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => filters.IsSortDescending
                ? hotelQuery.OrderByDescending(h => h.Name)
                : hotelQuery.OrderBy(h => h.Name),
            "rating" => filters.IsSortDescending
                ? hotelQuery.OrderByDescending(h => h.Rating)
                : hotelQuery.OrderBy(h => h.Rating),

            _ => hotelQuery.OrderBy(h => h.Name)
        };

        var pagedHotels = await hotelQuery
            .AsNoTracking()
            .ProjectTo<GetHotelSlimDto>(mapper.ConfigurationProvider)
            .ToPageResultAsync(paginationParameters);

        var result = new GetCountryHotelsDto
        {
            Id = countryId,
            Name = countryName,
            Hotels = pagedHotels
        };

        return Result<GetCountryHotelsDto>.Success(result);
    }
    public async Task<Result<GetCountryDto>> GetCountryAsync(int id)
    {

        var country = await context.Countries
            .AsNoTracking()
            .Where(c => c.CountryId == id)
            .ProjectTo<GetCountryDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return country is null
            ? Result<GetCountryDto>.Failure(new Error(ErrorCodes.NotFound, $"Country {id} was not found. "))
            : Result<GetCountryDto>.Success(country);
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
    public async Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto countryDto)
    {
        try
        {
            var exists = await CountryExistsAsync(countryDto.Name);
            if (exists)
            {
                return Result<GetCountryDto>
                    .Failure(new Error(ErrorCodes.Conflict, $"Country with the name: {countryDto.Name} already exists"));
            }

            var country = mapper.Map<Country>(countryDto);
            context.Countries.Add(country);
            await context.SaveChangesAsync();

            var resultDto = mapper.Map<GetCountryDto>(country);

            return Result<GetCountryDto>.Success(resultDto);
        }
        catch (Exception)
        {
            return Result<GetCountryDto>.Failure(new Error(ErrorCodes.Failure, "An unexpected errror occurred while creating the country."));
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

    public async Task<Result> PatchCountryAsync(int id, JsonPatchDocument<UpdateCountryDto> patchDto)
    {
        var country = await context.Countries.FindAsync(id);
        if(country is null)
            return Result.NotFound(new Error(ErrorCodes.NotFound, $"Country '{id}' was not found."));

        var countryDto = mapper.Map<UpdateCountryDto>(country);
        patchDto.ApplyTo(countryDto);

        if (countryDto.Id != id)
            return Result.BadRequest(new Error(ErrorCodes.Validation, $"Cannot modify the Id field."));

        string normalizedName = countryDto.Name.Trim().ToLower();
        var duplicatedExists = await context.Countries
            .AnyAsync(c => c.Name.Trim().ToLower() == normalizedName && c.CountryId != id);
        if(duplicatedExists)
            return Result.BadRequest(new Error(ErrorCodes.Conflict, $"Country with name '{countryDto.Name}' is already exists. "));

        mapper.Map(countryDto,country);

        // Questo nel caso in cui il tracking e` stato attivato globally (Nel Program.cs)
        // Quindi devo avvisare EF che l'entita` e` stata modificata
        context.Entry(country).State = EntityState.Modified;

        await context.SaveChangesAsync();

        return Result.Success();

    }
}
