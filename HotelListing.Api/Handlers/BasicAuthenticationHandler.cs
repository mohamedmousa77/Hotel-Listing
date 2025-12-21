using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace HotelListing.Api.Handlers;

public class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUserServices userServices
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var authHeader = authHeaderValues.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }
        var token = authHeader["Basic ".Length..].Trim();
        string decoded;
        try
        {
            var credentialsBytes = Convert.FromBase64String(token);
            decoded = Encoding.UTF8.GetString(credentialsBytes);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Basic Authorization token. ");

        }

        var separatorIndex = decoded.IndexOf(':');
        if (separatorIndex <= 0)
        {
            return AuthenticateResult.Fail("Invalid Basic Authorization credentials format. ");
        }

        var userNameOrEmail = decoded[..separatorIndex];
        var password = decoded[(separatorIndex + 1)..];

        var loginDto = new LoginUserDto
        {
            Email = userNameOrEmail,
            Password = password
        };

        var result = await userServices.LoginAsync(loginDto);

        if (!result.IsSuccess)
        {
            return AuthenticateResult.Fail("Invalid username or password. ");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userNameOrEmail)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
