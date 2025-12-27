using Azure;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Country;
using HotelListing.Api.Application.DTOs.Hotel;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Filtering;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("fixed")]
public class CountriesController(ICountriesService countriesService) : BaseApiController
{
    [HttpGet]
    [OutputCache(PolicyName = CasheConstants.AuthenticatedUserCashingPolicy)]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries(
        [FromQuery] CountryFilterParameters countryFilterParameters)
    {
        var result = await countriesService.GetCountriesAsync(countryFilterParameters);
        return ToActionResult(result);
    }

    [HttpGet("{countryId:int}/hotels")]
    public async Task<ActionResult<GetCountryHotelsDto>> GetCountryHotels(
        [FromRoute] int countryId, 
        [FromQuery] PaginationParameters paginationParameters,
        [FromQuery] CountryFilterParameters countryFilterParameters
        )
    {
        var result = await countriesService.GetCountryHotelsAsync(countryId, paginationParameters, countryFilterParameters);
        return ToActionResult(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry([FromQuery] int id)
    {
        var result = await countriesService.GetCountryAsync(id);
        return ToActionResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> PutCountry([FromQuery] int id, [FromBody] UpdateCountryDto countryDto)
    {

        var result = await countriesService.UpdateCountryAsync(id, countryDto);
        return ToActionResult(result);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> PatchCountry([FromQuery] int id, [FromBody] JsonPatchDocument<UpdateCountryDto> patchDto)
    {
        if (patchDto == null)
            return BadRequest("Patch document is required");

        var result = await countriesService.PatchCountryAsync(id, patchDto);
        return ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<ActionResult<Country>> PostCountry([FromBody]CreateCountryDto countryDto)
    {
        var result = await countriesService.CreateCountryAsync(countryDto);
        if (!result.IsSuccess) return MapErrorsToResponse(result.Errors);
        return CreatedAtAction(nameof(GetCountry), new { id = result.Value!.Id }, result.Value);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> DeleteCountry([FromQuery] int id)
    {
        var result = await countriesService.DeleteCountryAsync(id);
        return ToActionResult(result);
    }
}
