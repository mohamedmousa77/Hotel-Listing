using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Data.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasIndex(k => k.Key).IsUnique();
        builder.HasData(
            new ApiKey
            {
                Id = 1,
                AppName = "app",
                CreatedAtUtc = new DateTime(2025, 01, 01),
                Key = ""
            },
            new IdentityRole
            {
                Id = "6b20a298-1bd2-492d-bdad-510b15fc4366",
                Name = "User",
                NormalizedName = "USER"
            }
        );
    }
}
