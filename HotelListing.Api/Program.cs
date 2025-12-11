using HotelListing.Api.Constants;
using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Handlers;
using HotelListing.Api.MappingProfiles;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<HotelListingDbContext>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(
    options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
            ClockSkew = TimeSpan.Zero,
        };
    })
.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(AuthenticationDefaults.DefaultScheme, _ => {  })
.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(AuthenticationDefaults.ApiKeyScheme, _ => { });

builder.Services.AddAuthorization();

builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IHotelsService, HotelsService>();
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<HotelMappingProfile>();
    cfg.AddProfile<CountryMappingProfile>();
});

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
