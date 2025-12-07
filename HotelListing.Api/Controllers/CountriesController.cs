using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Country;
using HotelListing.Api.DTOs.Hotel;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController(HotelListingDbContext context) : ControllerBase
{
    private readonly HotelListingDbContext _context = context;

    // GET: api/Countries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReadCountriesDto>>> GetCountries()
    {
        var countries = await _context.Countries
            .Select(c => new ReadCountryDto(
                c.CountryId,
                c.Name,
                c.ShortName,
                c.Hotels.Select(h => new GetHotelSlimDto(
                    h.Id,
                    h.Name,
                    h.Address,
                    h.Rating
                    )).ToList()
            )).ToListAsync();

        return Ok(countries);
    }

    // GET: api/Countries/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ReadCountryDto>> GetCountry(int id)
    {
        var country = await _context.Countries
            .Where(c => c.CountryId == id)
            .Select(c => new ReadCountryDto(
                c.CountryId,
                c.Name,
                c.ShortName,
                c.Hotels.Select (h => new GetHotelSlimDto(
                    h.Id,
                    h.Name,
                    h.Address,
                    h.Rating        
                )).ToList()
            )).FirstOrDefaultAsync();

        if (country == null)
        {
            return NotFound();
        }

        return country;
    }

    // PUT: api/Countries/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCountry(int id, [FromBody] UpdateCountryDto countryDto)
    {
        if (id != countryDto.Id)
        {
            return BadRequest();
        }

        var country = await _context.Countries.FindAsync(id);
        if (country == null)
        {
            return NotFound();
        }

        country.Name = countryDto.Name;
        country.ShortName = countryDto.ShortName;

        _context.Entry(country).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CountryExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Countries
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Country>> PostCountry(CreateCountryDto countryDto)
    {
        var country = new Country
        {
            Name = countryDto.Name,
            ShortName = countryDto.ShortName
        };

        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCountry", new { id = country.CountryId }, country);
    }

    // DELETE: api/Countries/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country == null)
        {
            return NotFound();
        }

        _context.Countries.Remove(country);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CountryExists(int id)
    {
        return _context.Countries.Any(e => e.CountryId == id);
    }
}
