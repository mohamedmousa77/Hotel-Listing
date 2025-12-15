using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Bookings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/hotels/{hotelId:int}/bookings")]
[ApiController]
public class HotelBookingsContrller(IBookingServices bookingServices) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetBookingsDto>>>  GetBookings([FromRoute] int hotelId)
    {
        var result = await bookingServices.GetBookingsAsync(hotelId);
        return ToActionResult(result);
    }

    public async Task<ActionResult<GetBookingsDto>> CreateBooking([FromRoute] int hotelId, [FromBody] CreateBookingDto newBooking)
    {
        var result = await bookingServices.CreateBookingAsync(newBooking);
        return ToActionResult(result);
    } 
}
