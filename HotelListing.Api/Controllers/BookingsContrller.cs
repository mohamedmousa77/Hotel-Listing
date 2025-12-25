using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Bookings;
using HotelListing.Api.AuthorizationFilters;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/hotels/{hotelId:int}/bookings")]
[ApiController]
[Authorize]
public class BookingsContrller(IBookingServices bookingServices) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetBookingsDto>>> GetBookings(
        [FromRoute] int hotelId,
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] BookingFilterParameters filterParameters)
    {
        var result = await bookingServices.GetUserBookingsAsync(hotelId, paginationParameters, filterParameters);
        return ToActionResult(result);
    }
    [HttpPost]
    public async Task<ActionResult<GetBookingsDto>> CreateBooking([FromRoute] int hotelId, [FromBody] CreateBookingDto newBooking)
    {
        var result = await bookingServices.CreateBookingAsync(newBooking);
        return ToActionResult(result);
    }

    [HttpGet("admin")]
    [HotelOrSystemAdmin]
    public async Task<ActionResult<PagedResult<GetBookingsDto>>> GetBookingsForAdmin(
        [FromRoute] int hotelId,
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] BookingFilterParameters filterParameters)
    {
        var result = await bookingServices.GetBookingsForHotelAsync(hotelId,paginationParameters,filterParameters );
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
    [HotelOrSystemAdmin]
    public async Task<ActionResult> ConfirmBookingByAdmin(
        [FromRoute] int hotelId,
        [FromRoute] int bookingId)
    {
        var result = await bookingServices.ConfirmBookingByAdminAsync(hotelId, bookingId);
        return ToActionResult(result);
    }

    [HttpPut("{bookingId:int}/admin/cancel")]
    [HotelOrSystemAdmin]
    public async Task<ActionResult> CancelBookingByAdmin(
        [FromRoute] int hotelId,
        [FromRoute] int bookingId)
    {
        var result = await bookingServices.CancelBookingByAdminAsync(hotelId, bookingId);
        return ToActionResult(result);
    }

}
