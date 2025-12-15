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
    [HttpPost]
    public async Task<ActionResult<GetBookingsDto>> CreateBooking([FromRoute] int hotelId, [FromBody] CreateBookingDto newBooking)
    {
        var result = await bookingServices.CreateBookingAsync(newBooking);
        return ToActionResult(result);
    }

    [HttpPut("{bookingId:int}")]
    public async Task<ActionResult<GetBookingsDto>> UpdateBooking(
        [FromRoute] int hotelId, 
        [FromRoute] int bookingId, 
        [FromBody] UpdateBookingDto updatedBookingDto)
    {
        var result = await bookingServices.UpdateBookingAsync(hotelId, bookingId, updatedBookingDto);
        return ToActionResult(result);
    }

    [HttpPut("{bookingId:int}/cancel")]
    public async Task<ActionResult> CancelBooking(
        [FromRoute] int hotelId,
        [FromRoute] int bookingId)
    {
        var result = await bookingServices.CancelBookingAsync(hotelId, bookingId);
        return ToActionResult(result);
    }

    [HttpPut("{bookingId:int}/admin/confirm")]
    public async Task<ActionResult> ConfirmBookingByAdmin(
        [FromRoute] int hotelId,
        [FromRoute] int bookingId)
    {
        var result = await bookingServices.ConfirmBookingByAdminAsync(hotelId, bookingId);
        return ToActionResult(result);
    }

    [HttpPut("{bookingId:int}/admin/cancel")]
    public async Task<ActionResult> CancelBookingByAdmin(
        [FromRoute] int hotelId,
        [FromRoute] int bookingId)
    {
        var result = await bookingServices.CancelBookingByAdminAsync(hotelId, bookingId);
        return ToActionResult(result);
    } 

}
