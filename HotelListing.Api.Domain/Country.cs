using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Domain;

public class Country
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public IList<Hotel> Hotels { get; set; } = new List<Hotel>();
}
