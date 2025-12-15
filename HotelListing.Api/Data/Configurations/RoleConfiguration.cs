using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "6d8c41bb-a97e-4005-9ea6-b40f61387d4d",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            },
            new IdentityRole
            {
                Id = "6b20a298-1bd2-492d-bdad-510b15fc4366",
                Name = "User",
                NormalizedName = "USER"
            },
            new IdentityRole
            {
                Id = "6b20a298-a97e-492d-bdad-b40f61387d4d",
                Name = "Admin",
                NormalizedName = "HOTEL ADMIN"
            }
        );
    }
}
