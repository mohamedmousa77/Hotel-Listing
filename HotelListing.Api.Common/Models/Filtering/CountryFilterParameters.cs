namespace HotelListing.Api.Common.Models.Filtering;

public class CountryFilterParameters : BaseFilterParameters
{
    public string? CountryName { get; set; }
    public string? HasHotels { get; set; }
}