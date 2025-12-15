namespace HotelListing.Api.DTOs.Bookings;

public record UpdateBookingDto(
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Guests
);