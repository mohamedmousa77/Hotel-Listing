namespace HotelListing.Api.DTOs.Hotel;


public record GetHotelDto(
    int Id,
    string Name,
    string Address,
    double Rating,
    string Country,
    int CountryId
);
public record GetHotelSlimDto(
    int Id,
    string Name,
    string Address,
    double Rating
);