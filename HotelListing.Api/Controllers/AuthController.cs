using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous] // Anyone can access this contrller
public class AuthController(IUserServices userServices) : BaseApiController
{

    [HttpPost("register")]
    public async Task<ActionResult<RegisteredUserDto>> Register(RegisterUserDto registerUserDto)
    {
        var result = await userServices.RegisterAsync(registerUserDto);
        return ToActionResult(result);

    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginUserDto loginUserDto)
    {
        var result = await userServices.LoginAsync(loginUserDto);
        return ToActionResult(result);
    }
}
