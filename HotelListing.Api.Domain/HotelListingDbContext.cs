using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Reflection;
using System.Reflection.Emit;

namespace HotelListing.Api.Domain;

public class HotelListingDbContext(DbContextOptions<HotelListingDbContext> options) :
    IdentityDbContext<ApplicationUser>(options)
{

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Country> Countries { get; set; }

    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<HotelAdmin> HotelAdmins { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<Hotel>()
            .Property(h => h.PerNightRate)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Country>()
            .HasIndex(c => c.Name)
            .HasDatabaseName("IX_Countries_Name");

        builder.Entity<Country>()
            .HasIndex(c => c.ShortName)
            .HasDatabaseName("IX_Countries_ShortName");

        builder.Entity<Hotel>()
            .HasIndex(h => h.Name)
            .HasDatabaseName("IX_Hotel_Name");

        builder.Entity<Hotel>()
            .HasIndex(h => h.CountryId)
            .HasDatabaseName("IX_Hotel_CountryId");

        builder.Entity<Hotel>()
            .HasIndex(h => new { h.CountryId, h.Rating})
            .HasDatabaseName("IX_Hotel_CountryId_Rating");

    }
}
