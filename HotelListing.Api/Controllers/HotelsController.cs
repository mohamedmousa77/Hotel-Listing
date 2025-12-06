using HotelListing.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{

    private static List<Hotel> hotels = new List<Hotel>
    {
        new Hotel { Id = 1, Name = "Hotel One", Address = "123 Main St", Rating = 4.5 },
        new Hotel { Id = 2, Name = "Hotel Two", Address = "456 Elm St", Rating = 4.0 },
        new Hotel { Id = 3, Name = "Hotel Three", Address = "789 Oak St", Rating = 3.5 }
    };


    // GET: api/<HotelController>
    [HttpGet]
    public ActionResult<IEnumerable<Hotel>> Get()
    {
        return Ok(hotels);
    }

    // GET api/<HotelController>/5
    [HttpGet("{id}")]
    public ActionResult<Hotel> Get(int id)
    {

        Hotel? hotel = hotels.FirstOrDefault(h => h.Id == id);

        if (hotel == null)
        {
            return NotFound();
        }

        return Ok(hotel);
    }

    // POST api/<HotelController>
    [HttpPost]
    public ActionResult<Hotel> Post([FromBody] Hotel newHotel)
    {
        if(hotels.Any(h => h.Id == newHotel.Id))
        {
            return Conflict("A hotel with the same ID already exists.");
        }

        hotels.Add(newHotel);
        return CreatedAtAction(nameof(Get), new { id = newHotel.Id }, newHotel);
    }

    // PUT api/<HotelController>/5
    [HttpPut("{id}")]
    public ActionResult Put(int id, [FromBody] Hotel updatedHotel)
    {
        Hotel? existingHotel = hotels.FirstOrDefault(h => h.Id == id);

        if (existingHotel == null)
        {
           return NotFound();
        }
        existingHotel.Name = updatedHotel.Name;
        existingHotel.Address = updatedHotel.Address;
        existingHotel.Rating = updatedHotel.Rating;

        return NoContent();
    }

    // DELETE api/<HotelController>/5
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        Hotel? hotel = hotels.FirstOrDefault(h => h.Id == id);
        if (hotel == null)
        {
            return NotFound(new {message = "Hotel not found"});
        }
        hotels.Remove(hotel);
        return NoContent();
    }
}
