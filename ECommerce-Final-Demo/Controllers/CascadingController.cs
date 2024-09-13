using ECommerce_Final_Demo.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CascadingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CascadingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Countrys")]
        public async Task<IActionResult> GetCountries()
        {
            var Countrys = await _context.Countrys.ToListAsync();
            return Ok(Countrys);
        }

        [HttpGet("States")]
        public async Task<IActionResult> GetStates([FromQuery] int countryId)
        {
            var states = await _context.States
                .Where(s => s.CountryId == countryId)
                .ToListAsync();

            return Ok(states);
        }
        [HttpGet("Cities")]
        public async Task<IActionResult> GetCities([FromQuery] int stateId)
        {
            var cities = await _context.Citys
                .Where(c => c.StateId == stateId)
                .ToListAsync();

            return Ok(cities);
        }
        [HttpGet("countries/{id}")]
        public async Task<ActionResult<Country>> GetCountryById(int id)
        {
            var country = await _context.Countrys.FindAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            return Ok(country);
        }

        [HttpGet("states/{id}")]
        public async Task<ActionResult<State>> GetStateById(int id)
        {
            var state = await _context.States.FindAsync(id);

            if (state == null)
            {
                return NotFound();
            }

            return Ok(state);
        }

        [HttpGet("cities/{id}")]
        public async Task<ActionResult<City>> GetCityById(int id)
        {
            var city = await _context.Citys.FindAsync(id);

            if (city == null)
            {
                return NotFound();
            }

            return Ok(city);
        }
    }
}
