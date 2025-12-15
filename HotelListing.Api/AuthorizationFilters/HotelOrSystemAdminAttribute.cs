using HotelListing.Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HotelListing.Api.AuthorizationFilters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class HotelOrSystemAdminAttribute : TypeFilterAttribute
{
    public HotelOrSystemAdminAttribute() : base(typeof(HotelOrSystemAdminFilter))
    {
    }
}

public class HotelOrSystemAdminFilter(HotelListingDbContext dbContext) : IAuthorizationFilter
{
    public async void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if(!user.Identity?.IsAuthenticated == false)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        //If user is global administrator, allow
        if (user.IsInRole("Administrator"))
        {
            return;
        }
                
        var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
           ?? user.
                FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            context.Result = new ForbidResult();
            return;
        }

        context.RouteData.Values.TryGetValue("hotelId", out var hotelIdObj);
        int.TryParse(hotelIdObj?.ToString(), out int hotelID);
        
        if(hotelID == 0)
        {
            context.Result = new ForbidResult();
            return;
        }

        var isHotelAdmin = await dbContext.HotelAdmins
            .AnyAsync(hu => hu.HotelId == hotelID && hu.UserId == userId);
        if(!isHotelAdmin)
        {
            context.Result = new ForbidResult();
            return;
        }

    }
}
