using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

public class ApiKeyValidatorService(HotelListingDbContext db) : IApiKeyValidatorService
{
    public async Task<bool> IsValidAsync(string apiKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return false;

        var apiKeyEntity = await db.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == apiKey, ct);

        if (apiKeyEntity == null) return false;



        return apiKeyEntity.IsActive;
    }
}