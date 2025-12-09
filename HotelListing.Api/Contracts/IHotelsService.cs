using HotelListing.Api.DTOs.Hotel;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Contracts;

public interface IHotelsService
{
    Task<bool> HotelExistsAsync(int id);
    Task<bool> HotelExistsAsync(string name);
    Task<GetHotelDto> CreateHotelAsync(CreateHotelDto HotelDto);
    Task DeleteHotelAsync(int id);
    Task<IEnumerable<GetHotelDto>> GetHotelsAsync();
    Task<GetHotelDto> GetHotelAsync(int id);
    Task UpdateHotelAsync(int id, [FromBody] UpdateHotelDto HotelDto);
}