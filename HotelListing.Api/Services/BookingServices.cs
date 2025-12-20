using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Data.Enums;
using HotelListing.Api.DTOs.Bookings;
using HotelListing.Api.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens.Experimental;
using System.IdentityModel.Tokens.Jwt;

namespace HotelListing.Api.Services;

public class BookingServices(HotelListingDbContext context, IUserServices userServices, IMapper mapper) : IBookingServices
{
    public async Task<Result<IEnumerable<GetBookingsDto>>> GetBookingsForHotelAsync(int hotelId)
    {
        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);
        if (!hotelExists)        
            return Result<IEnumerable<GetBookingsDto>>.NotFound();
        

        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId)
            .OrderBy(b => b.CheckIn)
            .ProjectTo<GetBookingsDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<IEnumerable<GetBookingsDto>>.Success(bookings);
    }

    public async Task<Result<IEnumerable<GetBookingsDto>>> GetUserBookingsAsync(int hotelId)
    {
        var userId = userServices.UserId;

        var hotelExists = await context.Bookings.AnyAsync(h => h.Id == hotelId && h.UserId == userId);
        if (!hotelExists)
             return Result<IEnumerable<GetBookingsDto>>.NotFound();
        
        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId)
            .OrderBy(b => b.CheckIn)
            .ProjectTo<GetBookingsDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<IEnumerable<GetBookingsDto>>.Success(bookings);
    }
    public async Task<Result<GetBookingsDto>> CreateBookingAsync (CreateBookingDto newBookingDto)
    {
        var userId = userServices.UserId;
          
        var overlaps = await IsOverLap(newBookingDto.HotelId, newBookingDto.CheckInDate, newBookingDto.CheckOutDate, userId);
        if (overlaps)
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Conflict, "You already have a booking that overlaps with the selected dates."));

        var hotel = await context.Hotels
            .Where(h => h.Id == newBookingDto.HotelId)
            .FirstOrDefaultAsync();
        if (hotel is null)        
            return Result<GetBookingsDto>.NotFound();
        

        var nights = newBookingDto.CheckOutDate.DayNumber - newBookingDto.CheckInDate.DayNumber;
        var totalPrice = nights * hotel.PerNightRate;
        var booking = mapper.Map<Booking>(newBookingDto);
        booking.UserId = userId;
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var result = mapper.Map<GetBookingsDto>(booking);

        return Result<GetBookingsDto>.Success(result);
    }

    public async Task<Result<GetBookingsDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto updatedBookingDto)
    {
        var userId = userServices.UserId;

        var overlaps =await IsOverLap(hotelId, updatedBookingDto.CheckInDate, updatedBookingDto.CheckOutDate, userId);

        if (overlaps)
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Conflict, "You already have a booking that overlaps with the selected dates."));

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.HotelId == hotelId && b.UserId == userId);
        if (booking is null)
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found!"));
        
        if(booking.Status == BookingStatus.Canceled)
            return Result<GetBookingsDto>.Failure(new Error(ErrorCodes.Conflict, "Cannot update a canceled booking."));

        mapper.Map(updatedBookingDto, booking);
        var nights = updatedBookingDto.CheckOutDate.DayNumber - updatedBookingDto.CheckInDate.DayNumber;
        var perNightRate = booking.Hotel!.PerNightRate;
        booking.TotalPrice = nights * perNightRate;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var updated = mapper.Map<GetBookingsDto>(booking);

        return Result<GetBookingsDto>.Success(updated);

    }

    private async Task<bool> IsOverLap(int hotelId, DateOnly checkIn, DateOnly checkOut, string userId, int? bookingId = null)
    {
        var query =  context.Bookings
            .Where(
                b => b.HotelId == hotelId
                && b.Status != BookingStatus.Canceled
                && b.CheckIn < checkOut
                && b.CheckOut > checkIn
                && b.UserId == userId
            ).AsQueryable();

        if(bookingId.HasValue)
        {
            query = query.Where(b => b.Id != bookingId.Value);
        }

        return await query.AnyAsync();

    }

    public async Task<Result> CancelBookingAsync(int hotelId, int bookingId)
    {
        var userId = userServices.UserId;

        if (string.IsNullOrEmpty(userId))
            return Result.Failure(new Error(ErrorCodes.Validation, "User is not authenticated."));


        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.HotelId == hotelId && b.UserId == userId);


        if (booking is null)
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found!"));

        if (booking.Status == BookingStatus.Canceled)
            return Result.Failure(new Error(ErrorCodes.Conflict, "Cannot cancel a canceled booking."));

        booking.Status = BookingStatus.Canceled;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Result.Success();

    }

    public async Task<Result> CancelBookingByAdminAsync(int hotelId, int bookingId)
    {
        var userId = userServices.UserId;

        var isHotelAdmin = await context.HotelAdmins
            .AnyAsync(ha => ha.HotelId == hotelId && ha.UserId == userId);

        if (!isHotelAdmin)
            return Result.Failure(new Error(ErrorCodes.Forbid, "You are not the admin for the selected hotel."));

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.HotelId == hotelId);


        if (booking is null)
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found!"));

        if (booking.Status == BookingStatus.Canceled)
            return Result.Failure(new Error(ErrorCodes.Conflict, "Cannot cancel a canceled booking."));

        booking.Status = BookingStatus.Canceled;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ConfirmBookingByAdminAsync(int hotelId, int bookingId)
    {
        var userId = userServices.UserId;

        var isHotelAdmin = await context.HotelAdmins
            .AnyAsync(ha => ha.HotelId == hotelId && ha.UserId == userId);

        if (!isHotelAdmin)
            return Result.Failure(new Error(ErrorCodes.Forbid, "You are not the admin for the selected hotel."));

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.HotelId == hotelId);


        if (booking is null)
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found!"));

        if (booking.Status == BookingStatus.Canceled)
            return Result.Failure(new Error(ErrorCodes.Conflict, "Cannot cancel a canceled booking."));

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Result.Success();
    }


}
