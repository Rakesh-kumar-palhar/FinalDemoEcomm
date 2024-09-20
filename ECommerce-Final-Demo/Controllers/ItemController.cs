using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using ECommerce_Final_Demo.Services;
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
    [Authorize]
    public class ItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService _logger;
        public ItemController(ApplicationDbContext context, ILoggerService logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("allItem")]
        
        public async Task<IActionResult> GetItem()
        {
            try
            {
                var items = await _context.Items
                    .Include(u => u.Store)
                    .Where(i => !i.IsDelete)
                    .ToListAsync();

                var itemDtos = ItemDto.Mapping(items);

                return Ok(itemDtos);
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                return StatusCode(500, new { Message = "An error occurred while retrieving items." });
            }
        }

        [HttpGet("listofitem")]
        [Authorize(Roles = "SuperAdmin,StoreAdmin,User")]
        public async Task<IActionResult> GetItems(Guid storeId)
        {
            try
            {
                var items = await _context.Items
                    .Where(i => i.StoreId == storeId)
                    .Where(i => !i.IsDelete)
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
                _logger.Log(ex);
                return StatusCode(500, new { Message = "An error occurred while retrieving items." });
            }
        }

        [HttpPost("additem")]
         [Authorize(Roles = "SuperAdmin, StoreAdmin")]
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

                
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Item created successfully." });
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                return StatusCode(500, new { Message = "An error occurred while adding the item." });
            }
        }

        [HttpPut("updateitem/{itemId:guid}")]
        [Authorize(Roles = "SuperAdmin, StoreAdmin")]
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
                _logger.Log(ex);
                return StatusCode(500, new { Message = "An error occurred while updating the item." });
            }
        }

        [HttpDelete("deleteitem/{itemId:guid}")]
        [Authorize(Roles = "SuperAdmin, StoreAdmin")]
        public async Task<IActionResult> DeleteItem(Guid itemId)
        {
            try
            {
                var item = await _context.Items.FindAsync(itemId);

                if (item == null)
                {
                    return NotFound(new { Message = "Item not found." });
                }

               
                item.IsDelete = true;
                _context.Items.Update(item); 
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Item deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                return StatusCode(500, new { Message = "An error occurred while deleting the item." });
            }
        }

        [HttpGet("getdetailsbyid/{id:guid}")]
        [Authorize(Roles = "SuperAdmin,StoreAdmin")]
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
                _logger.Log(ex);
                return StatusCode(500, new { Message = "An error occurred while retrieving the item details." });
            }
        }
       
    }
}
