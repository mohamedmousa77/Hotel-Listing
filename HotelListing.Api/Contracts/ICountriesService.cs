using HotelListing.Api.DTOs.Country;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Contracts;

public interface ICountriesService
{
    Task<bool> CountryExistsAsync(int id);
    Task<bool> CountryExistsAsync(string name);
    Task<ReadCountryDto> CreateCountryAsync(CreateCountryDto countryDto);
    Task DeleteCountryAsync(int id);
    Task<IEnumerable<ReadCountriesDto>> GetCountriesAsync();
    Task<ReadCountryDto> GetCountryAsync(int id);
    Task UpdateCountryAsync(int id, [FromBody] UpdateCountryDto countryDto);
}