using HotelListing.Api.Application.DTOs.Hotel;
using HotelListing.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Application.Contracts
{
    public interface IHotelsService
    {
        Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto);
        Task<Result> DeleteHotelAsync(int id);
        Task<Result<GetHotelDto>> GetHotelAsync(int id);
        Task<Result<IEnumerable<GetHotelDto>>> GetHotelsAsync();
        Task<bool> HotelExistsAsync(int id);
        Task<bool> HotelExistsAsync(string name, int countryId);
        Task<Result> UpdateHotelAsync(int id, [FromBody] UpdateHotelDto hotelDto);
    }
}