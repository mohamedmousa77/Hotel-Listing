using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.Services;
using HotelListing.Api.CashePolicies;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Config;
using HotelListing.Api.Domain;
using HotelListing.Api.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Hotel Listing API Starting.");
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");

    builder.Services.AddDbContextPool<HotelListingDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.CommandTimeout(60);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        });
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }

        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }, poolSize: 128);

    builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
        .AddEntityFrameworkStores<HotelListingDbContext>();

    builder.Services.AddHttpContextAccessor();

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
        ?? new JwtSettings();
    if (string.IsNullOrWhiteSpace(jwtSettings.Key))
    {
        Log.Fatal("jwtSettings:Key is not configured");
        throw new InvalidOperationException("JWT Key is not configured.");
    }

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero,
            };
        })
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(AuthenticationDefaults.DefaultScheme, _ => { })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(AuthenticationDefaults.ApiKeyScheme, _ => { });

    builder.Services.AddAuthorization();

    builder.Services.AddScoped<ICountriesService, CountriesService>();
    builder.Services.AddScoped<IHotelsService, HotelsService>();
    builder.Services.AddScoped<IUserServices, UserServices>();
    builder.Services.AddScoped<IBookingServices, BookingServices>();
    builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();

    builder.Services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());

    builder.Services.AddControllers()
        .AddNewtonsoftJson()
        .AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    builder.Services.AddOpenApi();

    //builder.Services.AddMemoryCache();
    builder.Services.AddOutputCache(options =>
    {
        options.AddPolicy(CasheConstants.AuthenticatedUserCashingPolicy, policy =>
        {
            policy.AddPolicy<CustomCashePolicy>()
                .SetCacheKeyPrefix(CasheConstants.AuthenticatedUserCashingPolicyTag);
        }, true);
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("fixed", opt =>
        {
            opt.Window = TimeSpan.FromMinutes(1);
            opt.PermitLimit = 40;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 5;
        });

        options.AddPolicy("perUser", context =>
        {
            var userName = context.User?.Identity?.Name ?? "anonymous";

            return RateLimitPartition.GetSlidingWindowLimiter(userName, _ => new SlidingWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 50,
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 3
            });
        });

        // Global rate limit by IP
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            });
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
            }

            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";

            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = "Rate limit exceed. Please try again later",
                retryAfter = retryAfter.TotalSeconds,
            }, cancellationToken: cancellationToken);
        };
    });

    var app = builder.Build();

    app.MapGroup("api/defaultauth").MapIdentityApi<ApplicationUser>();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        // Development request logging to help debug client connectivity from the .http runner
        app.Use(async (context, next) =>
        {
            var logger = app.Logger;
            logger.LogInformation("Incoming request: {method} {path}", context.Request.Method, context.Request.Path + context.Request.QueryString);
            foreach (var header in context.Request.Headers)
            {
                logger.LogDebug("Header: {name}={value}", header.Key, header.Value.ToString());
            }

            await next();
            logger.LogInformation("Response status: {statusCode} for {method} {path}", context.Response.StatusCode, context.Request.Method, context.Request.Path);
        });
    }
    else
    {
        app.UseHttpsRedirection();
    }

    app.UseHttpsRedirection();

    app.UseRateLimiter();

    app.UseAuthorization();

    app.UseOutputCache();

    app.MapControllers();

    Log.Information("App Started successfully");

    app.Run();

}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

