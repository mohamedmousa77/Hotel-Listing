using Asp.Versioning;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Country;
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
/// <summary>
/// Controller for managing countries
/// </summary>
/// <param name="countriesService"></param>
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
[EnableRateLimiting("fixed")]
public class CountriesController(ICountriesService countriesService) : BaseApiController
{
    /// <summary>
    ///  Retrieves that list of countries that match the provided filter parameters.
    /// </summary>
    /// <param name="countryFilterParameters">An optional sef of parameters used to filter the list of countries</param>
    /// <returns>An async operation that returns an <see cref="ActionResult{T}"/></returns>
    /// cref="IEnumerable{GetCountriesDto}"/> containing the list of countries that match the provided filter parameters.
    /// <response code="200">Returns the list of countries</response>
    /// <response code="404">If the filter parameters are invalid</response>
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
    // Another way to update the documentation with the actual response types
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    public async Task<ActionResult<Country>> PostCountry([FromBody] CreateCountryDto countryDto)
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
