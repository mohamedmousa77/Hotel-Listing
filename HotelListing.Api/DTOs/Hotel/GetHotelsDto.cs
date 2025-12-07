namespace HotelListing.Api.DTOs.Hotel;

public record GetHotelsDto(
    int Id,
    string Name,
    string Address,
    double Rating,
    int CountryId
);
public record GetHotelDto(
    int Id,
    string Name,
    string Address,
    double Rating,
    string Country
);
public record GetHotelSlimDto(
    int Id,
    string Name,
    string Address,
    double Rating
);