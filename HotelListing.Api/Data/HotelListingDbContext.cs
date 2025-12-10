using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Data;

public class HotelListingDbContext : IdentityDbContext<ApplicationUser>
{
    public HotelListingDbContext(DbContextOptions<HotelListingDbContext> options) :
        base(options)
    {
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Country> Countries { get; set; }
}
