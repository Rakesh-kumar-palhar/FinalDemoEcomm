using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using ECommerce_Final_Demo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/stores/")]
    [ApiController]
    
    public class StoreController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StoreController> _logger;
        public StoreController(ApplicationDbContext context,Logger<StoreController> logger)
        {
            _context = context;
            _logger = logger;
        }


        //if you wants to see the list of the store
        [HttpGet("allstores")]
        //[Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetStores()
        {
            try {
                var stores = await _context.Stores
            .Where(s => !s.IsDelete)  
            .ToListAsync();

                var storeDtos = StoreDto.Mapping(stores);
                return Ok(storeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fatch list of store.");
                return StatusCode(500, new { Message = "An error occurred while fatch list of store." });
            }
        }
        //if you wants to add the store
        [HttpPost("addstore")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AddStore([FromBody] StoreDto storeDto)
        {
            try {
                if (storeDto == null)
                {
                    return BadRequest("Store data is required.");
                }
                var store = StoreDto.Mapping(storeDto);
                storeDto.Id = Guid.NewGuid();
                _context.Stores.Add(StoreDto.Mapping(storeDto));
                await _context.SaveChangesAsync();

                return Ok("Store created successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while add the store.");
                return StatusCode(500, new { Message = "An error occurred while add the store." });
            }
        }
        //if you wants to see the store by id

        [HttpGet("storedetails{storeId:guid}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetStoreById(Guid storeId)
        {
            try {
                var store = await _context.Stores.FindAsync(storeId);
                if (store == null)
                {
                    return NotFound("Store not found.");
                }

                var storeDto = StoreDto.Mapping(store);
                return Ok(storeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fatch the storedetails.");
                return StatusCode(500, new { Message = "An error occurred while fatch the storedetails." });
            }
        }   
        //if you wants to update the store
        [HttpPut("updatestore{storeId:guid}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateStore(Guid storeId, [FromBody] StoreDto storeDto)
        {
            try {
                var existingStore = await _context.Stores.FindAsync(storeId);

                if (existingStore == null)
                {
                    return NotFound(new { Message = "Store not found." });
                }
                existingStore.Name = storeDto.Name;
                existingStore.CountryId = storeDto.CountryId;
                existingStore.StateId = storeDto.StateId;
                existingStore.CityId = storeDto.CityId;
                existingStore.Image = storeDto.Image;

                _context.Stores.Update(existingStore);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Store updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the store.");
                return StatusCode(500, new { Message = "An error occurred while updating the store." });
            }
        }
        //if you wants to delete the store
        [HttpDelete("{storeId:guid}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteStore(Guid storeId)
        {
            try {
                var store = await _context.Stores.FindAsync(storeId);

                if (store == null)
                {
                    return NotFound(new { Message = "Store not found." });
                }

                
                store.IsDelete = true;
                _context.Stores.Update(store);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Store deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while delete the store.");
                return StatusCode(500, new { Message = "An error occurred while delete the store." });
            }
        }
       
    }
}
