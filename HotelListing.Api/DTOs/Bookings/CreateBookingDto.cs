namespace HotelListing.Api.DTOs.Bookings;

public record CreateBookingDto(
    int HotelId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Guests
);