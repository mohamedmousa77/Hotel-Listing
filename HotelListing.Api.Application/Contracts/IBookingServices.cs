using HotelListing.Api.Application.DTOs.Bookings;
using HotelListing.Api.Common.Results;

namespace HotelListing.Api.Application.Contracts;

public interface IBookingServices
{
    Task<Result> CancelBookingAsync(int hotelId, int bookingId);
    Task<Result> CancelBookingByAdminAsync(int hotelId, int bookingId);
    Task<Result> ConfirmBookingByAdminAsync(int hotelId, int bookingId);
    Task<Result<GetBookingsDto>> CreateBookingAsync(CreateBookingDto newBookingDto);
    Task<Result<IEnumerable<GetBookingsDto>>> GetBookingsForHotelAsync(int hotelId);
    Task<Result<IEnumerable<GetBookingsDto>>> GetUserBookingsAsync(int hotelId);
    Task<Result<GetBookingsDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updatedBooking);
}