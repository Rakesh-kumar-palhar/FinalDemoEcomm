using ECommerce_Final_Demo.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using ECommerce_Final_Demo.Services;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CascadingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CascadingController> _logger;
        public CascadingController(ApplicationDbContext context, ILogger<CascadingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("Countrys")]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                var countries = await _context.Countrys.ToListAsync();
                return Ok(countries);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while retrieving countries");
                return StatusCode(500, new { Message = "An error occurred while retrieving countries." });
            }
        }

        [HttpGet("States")]
        public async Task<IActionResult> GetStates([FromQuery] int countryId)
        {
            try
            {
                var states = await _context.States
                    .Where(s => s.CountryId == countryId)
                    .ToListAsync();

                if (states == null || !states.Any())
                {
                    return NotFound(new { Message = "No states found for the given country ID." });
                }

                return Ok(states);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while retrieving states");
                return StatusCode(500, new { Message = "An error occurred while retrieving states." });
            }
        }

        [HttpGet("Cities")]
        public async Task<IActionResult> GetCities([FromQuery] int stateId)
        {
            try
            {
                var cities = await _context.Citys
                    .Where(c => c.StateId == stateId)
                    .ToListAsync();

                if (cities == null || !cities.Any())
                {
                    return NotFound(new { Message = "No cities found for the given state ID." });
                }

                return Ok(cities);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while retrieving cities.");
                return StatusCode(500, new { Message = "An error occurred while retrieving cities." });
            }
        }

        [HttpGet("countries/{id}")]
        public async Task<ActionResult<Country>> GetCountryById(int id)
        {
            try
            {
                var country = await _context.Countrys.FindAsync(id);

                if (country == null)
                {
                    return NotFound(new { Message = "Country not found." });
                }

                return Ok(country);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while retrieving the country.");
                return StatusCode(500, new { Message = "An error occurred while retrieving the country." });
            }
        }

        [HttpGet("states/{id}")]
        public async Task<ActionResult<State>> GetStateById(int id)
        {
            try
            {
                var state = await _context.States.FindAsync(id);

                if (state == null)
                {
                    return NotFound(new { Message = "State not found." });
                }

                return Ok(state);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while retrieving the state.");
                return StatusCode(500, new { Message = "An error occurred while retrieving the state." });
            }
        }

        [HttpGet("cities/{id}")]
        public async Task<ActionResult<City>> GetCityById(int id)
        {
            try
            {
                var city = await _context.Citys.FindAsync(id);

                if (city == null)
                {
                    return NotFound(new { Message = "City not found." });
                }

                return Ok(city);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while retrieving the city.");
                return StatusCode(500, new { Message = "An error occurred while retrieving the city." });
            }
        }

       
    }
}
