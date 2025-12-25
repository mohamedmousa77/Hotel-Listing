using HotelListing.Api.Application.DTOs.Bookings;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Domain;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HotelListing.Api.Application.Contracts;

public interface IBookingServices
{
    Task<Result<PagedResult<GetBookingsDto>>> GetBookingsForHotelAsync(
        int hotelId, PaginationParameters paginationParameters, BookingFilterParameters bookingFilterParameters);
    Task<Result<PagedResult<GetBookingsDto>>> GetUserBookingsAsync(
        int hotelId, PaginationParameters paginationParameters, BookingFilterParameters bookingFilterParameters);
    IQueryable<Booking> ApplyFilters(int hotelId, BookingFilterParameters bookingFilterParameters);
    Task<Result> CancelBookingAsync(int hotelId, int bookingId);
    Task<Result> CancelBookingByAdminAsync(int hotelId, int bookingId);
    Task<Result> ConfirmBookingByAdminAsync(int hotelId, int bookingId);
    Task<Result<GetBookingsDto>> CreateBookingAsync(CreateBookingDto newBookingDto);
    Task<Result<GetBookingsDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updatedBooking);
}