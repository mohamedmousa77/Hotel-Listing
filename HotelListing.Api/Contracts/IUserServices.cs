using HotelListing.Api.DTOs.Auth;
using HotelListing.Api.Results;

namespace HotelListing.Api.Contracts;

public interface IUserServices
{
    string UserId { get; }

    Task<Result<string>> LoginAsync(LoginUserDto loginUserDto);
    Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
}