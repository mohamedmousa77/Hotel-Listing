using System.ComponentModel.DataAnnotations;

namespace HotelListing.Api.Application.DTOs.Hotel;

public class CreateHotelDto
{
    [Required]
    public required string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public required string Address { get; set; } = string.Empty;

    [Range(1,5)]
    public double Rating { get; set; }

    [Required]
    public int CountryId { get; set; }
}
