using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        private Guid GetUserId()
        {
            var userIdString = HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                throw new ArgumentNullException("User ID is null or empty in the HTTP context.");
            }

            return Guid.Parse(userIdString);
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
                var userId = GetUserId();  // Get the authenticated user's ID

                // Step 1: Find the user's cart
                var userCart = await _context.Carts
                    .Include(c => c.Items)  // Include the cart's items
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (userCart == null)
                {
                    return NotFound("No cart found for the user.");
                }

                // Step 2: Retrieve the cart items with item details and map them to CartItemDto
                var cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == userCart.Id)
                    .Include(ci => ci.Item)
                    .Select(ci => new CartItemDto
                    {
                        ItemId = ci.ItemId,
                        ItemName = ci.Item.Name,
                        Price = ci.price,
                        Quantity = ci.Quantity
                    })
                    .ToListAsync();

                if (cartItems == null || !cartItems.Any())
                {
                    return NotFound(new { Message = "Cart is empty." });
                }

                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                // Log the exception here if you have a logging service
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while retrieving cart items." });
            }
        }

        [HttpDelete("removeitem/{itemId:guid}")]
        public async Task<IActionResult> RemoveCartItem(Guid itemId)
        {
            try
            {
                var userId = GetUserId();  // Retrieve the authenticated user's ID

                var userCart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (userCart == null)
                {
                    return NotFound("No cart found for the user.");
                }

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == userCart.Id && ci.ItemId == itemId);

                if (cartItem == null)
                {
                    return NotFound("Item not found in the cart.");
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok("Item removed from cart.");
            }
            catch (Exception ex)
            {
                // Log the exception here if you have a logging service
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while removing the item from the cart." });
            }
        }

        [HttpPost("additem")]
        public async Task<IActionResult> AddItemToCart([FromBody] CartItemDto cartItemDto)
        {
            try
            {
                var userId = GetUserId();

                // Step 1: Check if the user already has a cart
                var existingCart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

                // Step 2: If the cart doesn't exist, create it
                if (existingCart == null)
                {
                    existingCart = new Cart { UserId = userId };
                    _context.Carts.Add(existingCart);
                    await _context.SaveChangesAsync();  // Save to generate CartId
                }

                // Step 3: Check if the item is already in the cart
                var existingCartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == existingCart.Id && ci.ItemId == cartItemDto.ItemId);

                if (cartItemDto.Quantity == 0)
                {
                    // Step 4: If quantity is 0, remove the item from the cart
                    if (existingCartItem != null)
                    {
                        _context.CartItems.Remove(existingCartItem);
                        await _context.SaveChangesAsync();
                        return Ok("Item removed from cart.");
                    }

                    return NotFound("Item not found in cart.");
                }

                if (existingCartItem != null)
                {
                    // Step 5: Update existing item quantity and price if item is already in the cart
                    existingCartItem.Quantity = cartItemDto.Quantity;
                    existingCartItem.price = cartItemDto.Price;
                    _context.CartItems.Update(existingCartItem);
                }
                else
                {
                    // Step 6: If the item is not in the cart, add it
                    var cartItem = new CartItem
                    {
                        CartId = existingCart.Id,
                        ItemId = cartItemDto.ItemId,
                        Quantity = cartItemDto.Quantity,
                        price = cartItemDto.Price
                    };

                    _context.CartItems.Add(cartItem);
                }

                // Step 7: Save changes to the database
                await _context.SaveChangesAsync();

                return Ok("Item added/updated in cart.");
            }
            catch (Exception ex)
            {
                // Log the exception here if you have a logging service
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while adding/updating the item in the cart." });
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
