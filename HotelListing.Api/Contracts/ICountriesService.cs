using HotelListing.Api.DTOs.Country;
using HotelListing.Api.Results;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Services
{
    public interface ICountriesService
    {
        Task<bool> CountryExistsAsync(int id);
        Task<bool> CountryExistsAsync(string name);
        Task<Result<ReadCountryDto>> CreateCountryAsync(CreateCountryDto countryDto);
        Task<Result> DeleteCountryAsync(int id);
        Task<Result<IEnumerable<ReadCountriesDto>>> GetCountriesAsync();
        Task<Result<ReadCountryDto>> GetCountryAsync(int id);
        Task<Result> UpdateCountryAsync(int id, [FromBody] UpdateCountryDto countryDto);
    }
}