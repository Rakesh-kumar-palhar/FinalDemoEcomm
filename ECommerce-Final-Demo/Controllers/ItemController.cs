using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        // [Authorize(Roles = "SuperAdmin ")]
        public async Task<IActionResult> GetItem()
        {
            // var Item = await _context.Items.ToListAsync();
            var Item = await _context.Items
            .Include(u => u.Store)
            .ToListAsync();

            var ItemDtos = ItemDto.Mapping(Item);

            return Ok(ItemDtos);
        }
        [HttpGet("listofitem")]
        //[Authorize]
        public async Task<IActionResult> GetItems(Guid storeId)
        {
            // Get items filtered by storeId
            var items = await _context.Items
                                      .Where(i => i.StoreId == storeId)
                                      .ToListAsync();

            // Map the items to DTOs
            var itemDtos = ItemDto.Mapping(items);

            return Ok(itemDtos);
        }

        //if you wants to add item 
        [HttpPost("additem")]
        //[Authorize(Roles = "Super Admin, Store Admin")]
        public async Task<IActionResult> AddItem([FromBody] ItemDto itemDto)
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

           
            return Ok("item created succesfully");
        }
        //update item by id
        [HttpPut("updateitem{itemId:guid}")]
       // [Authorize(Roles = "Super Admin, Store Admin")]
        public async Task<IActionResult> UpdateItem(Guid itemId, [FromBody] ItemDto itemDto)
        {
            if (itemId != itemDto.Id)
            {
                return BadRequest(new { Message = "Item ID mismatch." });
            }

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

        // Item delete by id
        [HttpDelete("deleteitem{itemId:guid}")]
        //[Authorize(Roles = "Super Admin, Store Admin")]
        public async Task<IActionResult> DeleteItem(Guid itemId)
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

        //item details by id
        [HttpGet("getdetailsbyid{id}")]
        public async Task<ActionResult<ItemDto>> GetItemById(Guid id)
        {
            var item = await _context.Items
                .Include(i => i.Store)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            var itemDto = ItemDto.Mapping(item); 
            return Ok(itemDto); 
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchItems([FromQuery] ItemFilterDto filterDto)
        {
            var query = _context.Items.AsQueryable();

            if (filterDto.Category.HasValue)
            {
                query = query.Where(i => i.Type == filterDto.Category.Value);
            }

            var items = await query.ToListAsync();

            return Ok(items);
        }
    }
}
