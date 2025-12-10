using HotelListing.Api.Contracts;

namespace HotelListing.Api.Services;

public class ApiKeyValidatorService(IConfiguration configuration) : IApiKeyValidatorService
{
    public Task<bool> IsValidAsync(string apiKey, CancellationToken ct = default)
    {
        return Task.FromResult(apiKey.Equals(configuration["ApiKey"]));
    }
}