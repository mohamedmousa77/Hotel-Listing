using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Hotel;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HotelsController(HotelsService hotelsService) : BaseApiController
{

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetHotelDto>>> GetHotels()
    {
        var hotels = await hotelsService.GetHotelsAsync();
        return ToActionResult(hotels);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var hotel = await hotelsService.GetHotelAsync(id);
        return ToActionResult(hotel);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto hotelDto)
    {
        var result = await hotelsService.UpdateHotelAsync(id, hotelDto);
        return ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<Hotel>> PostHotel(CreateHotelDto hotelDto)
    {
        var hotel = await hotelsService.CreateHotelAsync(hotelDto);
        if (!hotel.IsSuccess)        
            return MapErrorsToResponse(hotel.Errors);
        
        return CreatedAtAction("GetHotel", new { id = hotel.Value!.Id }, hotel.Value);
    }

    // DELETE: api/Hotels/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var result = await hotelsService.DeleteHotelAsync(id);
        return ToActionResult(result);
    }
}
