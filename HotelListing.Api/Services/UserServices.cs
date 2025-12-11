using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Auth;
using HotelListing.Api.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.Api.Services;

public class UserServices(UserManager<ApplicationUser> userManager, IConfiguration configuration) : IUserServices
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

        var registeredUserDto = new RegisteredUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
        return Result<RegisteredUserDto>.Success(registeredUserDto);
    }


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
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName)
        };

        // set user role claims 
        var roles = await userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
        claims = claims.Union(roleClaims).ToList();

        // Set JWT credentials key
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // create an encoded token
        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(configuration["JwtSettings:Issuer"])),
            signingCredentials: credentials
        );

        // Return Token value
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
