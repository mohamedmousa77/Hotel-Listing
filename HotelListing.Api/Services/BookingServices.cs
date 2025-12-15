using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Data.Enums;
using HotelListing.Api.DTOs.Bookings;
using HotelListing.Api.Results;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace HotelListing.Api.Services;

public class BookingServices(HotelListingDbContext context, IHttpContextAccessor httpContextAccessor) : IBookingServices
{
    public async Task<Result<IEnumerable<GetBookingsDto>>> GetBookingsAsync(int hotelId)
    {
        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);
        if (!hotelExists)
        {
            return Result<IEnumerable<GetBookingsDto>>.NotFound();
        }

        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId)
            .OrderBy(b => b.CheckIn)
            .Select(b => new GetBookingsDto(
                 b.Id,
                 b.HotelId,
                 b.Guests,
                 b.Hotel!.Name,
                 b.CheckIn,
                 b.CheckOut,
                 b.TotalPrice,
                 b.Status.ToString(),
                 b.CreatedAtUtc,
                 b.UpdatedAtUtc)
            ).ToListAsync();

        return Result<IEnumerable<GetBookingsDto>>.Success(bookings);
    }

    public async Task<Result<GetBookingsDto>> CreateBookingAsync (CreateBookingDto newBookingDto)
    {
        var userId = httpContextAccessor?.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if(string.IsNullOrEmpty(userId))
             return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, "User is not authenticated."));

        var nights = newBookingDto.CheckOutDate.DayNumber - newBookingDto.CheckInDate.DayNumber;
        if (nights <= 0) 
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, "Check-out date must be after check-in date."));
           
        if(newBookingDto.Guests <= 0)
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Validation, "Number of guests must be at least 1."));

        var hotel = await context.Hotels
            .Where(h => h.Id == newBookingDto.HotelId)
            .FirstOrDefaultAsync();
        if (hotel is null)
        {
            return Result<GetBookingsDto>.NotFound();
        }

        var overlaps = await context.Bookings
            .AnyAsync(b => b.HotelId == newBookingDto.HotelId
                           && b.Status != BookingStatus.Canceled
                           && b.CheckIn < newBookingDto.CheckOutDate
                           && b.CheckOut > newBookingDto.CheckInDate 
                           && b.UserId == userId);

        if (overlaps)
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Conflict, "You already have a booking that overlaps with the selected dates."));
        
        var totalPrice = nights * hotel.PerNightRate;

        var booking = new Booking
        {
            HotelId = newBookingDto.HotelId,
            UserId = userId,
            Guests = newBookingDto.Guests,
            CheckIn = newBookingDto.CheckInDate,
            CheckOut = newBookingDto.CheckOutDate,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var createdBooking = new GetBookingsDto(
            booking.Id,
            hotel.Id,
            booking.Guests,
            hotel.Name,
            booking.CheckIn,
            booking.CheckOut,
            totalPrice,
            BookingStatus.Pending.ToString(),
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc
        );

        return Result<GetBookingsDto>.Success(createdBooking);
    }
}
