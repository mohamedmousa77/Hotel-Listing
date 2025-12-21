using HotelListing.Api.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Domain.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "6d8c41bb-a97e-4005-9ea6-b40f61387d4d",
                Name = RoleNames.Administrator,
                NormalizedName = RoleNames.Administrator.ToUpper(),
                ConcurrencyStamp = "11111111-1111-1111-1111-111111111111"
            },
            new IdentityRole
            {
                Id = "6b20a298-1bd2-492d-bdad-510b15fc4366",
                Name = RoleNames.User,
                NormalizedName = RoleNames.User.ToUpper(),
                ConcurrencyStamp = "22222222-2222-2222-2222-222222222222"
            },
            new IdentityRole
            {
                Id = "6b20a298-a97e-492d-bdad-b40f61387d4d",
                Name = RoleNames.HotelAdmin,
                NormalizedName = RoleNames.HotelAdmin.ToUpper(),
                ConcurrencyStamp = "22222222-2222-2222-2222-222222222222"
            }
        );
    }
}
