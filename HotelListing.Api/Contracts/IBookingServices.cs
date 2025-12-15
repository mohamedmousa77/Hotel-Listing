using HotelListing.Api.DTOs.Bookings;
using HotelListing.Api.Results;

namespace HotelListing.Api.Contracts;

public interface IBookingServices
{
    Task<Result<GetBookingsDto>> CreateBookingAsync(CreateBookingDto newBookingDto);
    Task<Result<IEnumerable<GetBookingsDto>>> GetBookingsAsync(int hotelId);
}