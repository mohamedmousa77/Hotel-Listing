namespace HotelListing.Api.DTOs.Bookings;

public record GetBookingsDto(
    int Id,
    int HotelId,
    int Guests,
    string HotelName,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    decimal TotalPrice,
    string Status,
    DateTime CreateAtUtc,
    DateTime? UpdatedAtUtc
);
