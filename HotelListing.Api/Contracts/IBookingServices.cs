using HotelListing.Api.DTOs.Bookings;
using HotelListing.Api.Results;

namespace HotelListing.Api.Contracts;

public interface IBookingServices
{
    Task<Result> CancelBookingAsync(int hotelId, int bookingId);
    Task<Result> CancelBookingByAdminAsync(int hotelId, int bookingId);
    Task<Result> ConfirmBookingByAdminAsync(int hotelId, int bookingId);
    Task<Result<GetBookingsDto>> CreateBookingAsync(CreateBookingDto newBookingDto);
    Task<Result<IEnumerable<GetBookingsDto>>> GetBookingsAsync(int hotelId);
    Task<Result<GetBookingsDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updatedBooking);
}