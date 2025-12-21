using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Auth;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Config;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.Api.Application.Services;

public class UserServices(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtOptions,
    IHttpContextAccessor httpContextAccessor, HotelListingDbContext context
    ) : IUserServices
{

    public async Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto)
    {
        var user = new ApplicationUser
        {
            UserName = registerUserDto.Email,
            Email = registerUserDto.Email,
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName
        };

        var result = await userManager.CreateAsync(user, registerUserDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new Error(e.Code, e.Description)).ToArray();
            return Result<RegisteredUserDto>.BadRequest(errors);

        }

        await userManager.AddToRoleAsync(user, registerUserDto.Role);

        // If Hotel Admin, add to HotelAdmins table
        if (registerUserDto.Role == RoleNames.HotelAdmin)
        {
            var hotelAdmin = context.HotelAdmins.Add(
                new HotelAdmin
                {
                    UserId = user.Id,
                    HotelId = registerUserDto.AssociatedHotelId.GetValueOrDefault()
                });
            await context.SaveChangesAsync();
        }
        var registeredUserDto = new RegisteredUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = registerUserDto.Role
        };
        return Result<RegisteredUserDto>.Success(registeredUserDto);
    }

    public string UserId
        => httpContextAccessor?.HttpContext?.User?.
                FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? httpContextAccessor?.HttpContext?.User?.
                FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? string.Empty;
    public async Task<Result<string>> LoginAsync(LoginUserDto loginUserDto)
    {
        var user = await userManager.FindByEmailAsync(loginUserDto.Email);
        if (user == null)
        {
            return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Invalid credentials!"));
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginUserDto.Password);
        if (!isPasswordValid)
        {
            return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Invalid credentials!"));
        }

        // Issue token
        string token = await GenerateToken(user);
        return Result<string>.Success(token);

    }

    public async Task<string> GenerateToken(ApplicationUser user)
    {
        // set basic user claims.
        var claims = new List<Claim>
        {
            new Claim (JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName)
        };

        // set user role claims 
        var roles = await userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
        claims = claims.Union(roleClaims).ToList();

        // Set JWT credentials key
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // create an encoded token
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtOptions.Value.DurationInMinutes)),
            signingCredentials: credentials
        );

        // Return Token value
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
