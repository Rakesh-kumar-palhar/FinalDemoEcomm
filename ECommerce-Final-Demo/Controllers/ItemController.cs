using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("allItem")]
        // [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetItem()
        {
            try
            {
                var items = await _context.Items
                    .Include(u => u.Store)
                    .ToListAsync();

                var itemDtos = ItemDto.Mapping(items);

                return Ok(itemDtos);
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while retrieving items." });
            }
        }

        [HttpGet("listofitem")]
        // [Authorize]
        public async Task<IActionResult> GetItems(Guid storeId)
        {
            try
            {
                var items = await _context.Items
                    .Where(i => i.StoreId == storeId)
                    .ToListAsync();

                if (items == null || !items.Any())
                {
                    return NotFound(new { Message = "No items found for the given store ID." });
                }

                var itemDtos = ItemDto.Mapping(items);

                return Ok(itemDtos);
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while retrieving items." });
            }
        }

        [HttpPost("additem")]
        // [Authorize(Roles = "Super Admin, Store Admin")]
        public async Task<IActionResult> AddItem([FromBody] ItemDto itemDto)
        {
            try
            {
                var item = ItemDto.Mapping(itemDto);

                if (await _context.Items.AnyAsync(i => i.Id == item.Id))
                {
                    return Conflict(new { Message = "Item with the same ID already exists." });
                }

                var storeExists = await _context.Stores.AnyAsync(s => s.Id == item.StoreId);
                if (!storeExists)
                {
                    return BadRequest(new { Message = "Store not found." });
                }

                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Item created successfully." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while adding the item." });
            }
        }

        [HttpPut("updateitem/{itemId:guid}")]
        // [Authorize(Roles = "Super Admin, Store Admin")]
        public async Task<IActionResult> UpdateItem(Guid itemId, [FromBody] ItemDto itemDto)
        {
            if (itemId != itemDto.Id)
            {
                return BadRequest(new { Message = "Item ID mismatch." });
            }

            try
            {
                var existingItem = await _context.Items.FindAsync(itemId);
                if (existingItem == null)
                {
                    return NotFound(new { Message = "Item not found." });
                }

                var storeExists = await _context.Stores.AnyAsync(s => s.Id == itemDto.StoreId);
                if (!storeExists)
                {
                    return BadRequest(new { Message = "Store not found." });
                }

                existingItem.Name = itemDto.Name;
                existingItem.Type = (Item.Category)itemDto.Category;
                existingItem.Price = itemDto.Price;
                existingItem.Image = itemDto.Image;
                existingItem.StoreId = itemDto.StoreId;

                _context.Items.Update(existingItem);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Item updated successfully." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while updating the item." });
            }
        }

        [HttpDelete("deleteitem/{itemId:guid}")]
        // [Authorize(Roles = "Super Admin, Store Admin")]
        public async Task<IActionResult> DeleteItem(Guid itemId)
        {
            try
            {
                var item = await _context.Items.FindAsync(itemId);

                if (item == null)
                {
                    return NotFound(new { Message = "Item not found." });
                }

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Item deleted successfully." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while deleting the item." });
            }
        }

        [HttpGet("getdetailsbyid/{id:guid}")]
        public async Task<ActionResult<ItemDto>> GetItemById(Guid id)
        {
            try
            {
                var item = await _context.Items
                    .Include(i => i.Store)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                {
                    return NotFound(new { Message = "Item not found." });
                }

                var itemDto = ItemDto.Mapping(item);
                return Ok(itemDto);
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while retrieving the item details." });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchItems([FromQuery] ItemFilterDto filterDto)
        {
            try
            {
                var query = _context.Items.AsQueryable();

                if (filterDto.Category.HasValue)
                {
                    query = query.Where(i => i.Type == filterDto.Category.Value);
                }

                var items = await query.ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while searching for items." });
            }
        }

        private async Task LogException(Exception ex)
        {
            // Log the exception details to a database or file
            var logger = new Logger
            {
                ExceptionType = ex.GetType().ToString(),
                Message = ex.Message
            };
            _context.Loggers.Add(logger);
            await _context.SaveChangesAsync();
        }
    }
}
