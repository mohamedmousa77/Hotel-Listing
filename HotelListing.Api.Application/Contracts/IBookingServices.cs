using HotelListing.Api.Application.DTOs.Bookings;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Results;

namespace HotelListing.Api.Application.Contracts;

public interface IBookingServices
{
    Task<Result> CancelBookingAsync(int hotelId, int bookingId);
    Task<Result> CancelBookingByAdminAsync(int hotelId, int bookingId);
    Task<Result> ConfirmBookingByAdminAsync(int hotelId, int bookingId);
    Task<Result<GetBookingsDto>> CreateBookingAsync(CreateBookingDto newBookingDto);
    Task<Result<PagedResult<GetBookingsDto>>> GetBookingsForHotelAsync(int hotelId, PaginationParameters paginationParameters);
    Task<Result<PagedResult<GetBookingsDto>>> GetUserBookingsAsync(int hotelId, PaginationParameters paginationParameters);
    Task<Result<GetBookingsDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updatedBooking);
}