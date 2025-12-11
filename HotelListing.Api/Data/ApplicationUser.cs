using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.Configuration;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.Api.Data
{
    public class ApplicationUser: IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [NotMapped] // To not allow to EF core to consider the following field as entity
        public string FullName => $"{FirstName}, {LastName}";
    }
}
