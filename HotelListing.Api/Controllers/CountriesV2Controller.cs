// #### This File just for testing purposes!! ####

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/v{version:apiVersion}/countries")]
[ApiController]
[ApiVersion("2.0", Deprecated = true)]
public class CountriesV2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult GetCounties(
        [FromQuery] int? pageNumber = 1,
        [FromQuery] int? pageSize = 10)
    {
        return Ok(new
        {
            Version = "v2.0",
            Message = "Enhanced countries endpoint with pagination... This is the deprecated version of the Countries API.",
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }
}
