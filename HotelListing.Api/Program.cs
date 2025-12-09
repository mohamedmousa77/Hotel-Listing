using HotelListing.Api.Contracts;
using HotelListing.Api.Data;
using HotelListing.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ICountriesService, CountriesService>();

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

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
