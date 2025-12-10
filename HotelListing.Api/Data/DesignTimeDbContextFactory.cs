using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HotelListing.Api.Data;
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HotelListingDbContext>
{
    public HotelListingDbContext CreateDbContext(string[] args)
    {
        // Build configuration (reads appsettings.json or environment variables)
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("HotelListingDbConnectionString")
                               ?? throw new InvalidOperationException("Connection string 'HotelListingDbConnectionString' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<HotelListingDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new HotelListingDbContext(optionsBuilder.Options);
    }
}