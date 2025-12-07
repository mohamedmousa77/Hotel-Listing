using HotelListing.Api.DTOs.Hotel;

namespace HotelListing.Api.DTOs.Country;

public record ReadCountriesDto (
    int Id,
    string Name,
    string ShortName
);
public record ReadCountryDto(
    int Id,
    string Name,
    string ShortName,
    List<GetHotelSlimDto> Hotels
);