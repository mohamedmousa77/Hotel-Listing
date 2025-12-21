using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Application.DTOs.Country;

public class CreateCountryDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [MaxLength(5)]
    public string ShortName { get; set; } = string.Empty;
}
