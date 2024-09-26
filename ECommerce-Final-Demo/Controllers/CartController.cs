using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using ECommerce_Final_Demo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CartController> _logger;
        public CartController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<CartController> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _logger = logger;
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
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
                var userId = GetUserId();                 
                var userCart = await _context.Carts
                    .Include(c => c.Items)  
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (userCart == null)
                {
                    return NotFound("No cart found for the user.");
                }                
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
                _logger.LogError(ex, "An error occurred during get cartitem");
                return StatusCode(500, new { Message = "An error occurred while retrieving cart items." });
            }
        }

        [HttpDelete("removeitem/{itemId:guid}")]
        [Authorize(Roles = "User")]
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
                _logger.LogError(ex, "An error occurred while removing the item from the cart.");
                return StatusCode(500, new { Message = "An error occurred while removing the item from the cart." });
            }
        }

        [HttpPost("additem")]
       [Authorize(Roles = "User")]
        public async Task<IActionResult> AddItemToCart([FromBody] CartItemDto cartItemDto)
        {
            try
            {
                var userId = GetUserId();

                // Find or create a cart for the user
                var existingCart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

                if (existingCart == null)
                {
                    // If no cart exists, create one
                    existingCart = new Cart { UserId = userId };
                    _context.Carts.Add(existingCart);
                    await _context.SaveChangesAsync();
                }

                // Check if the cart item already exists in the cart
                var existingCartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == existingCart.Id && ci.ItemId == cartItemDto.ItemId);

                // If quantity is 0, remove the item from the cart
                if (cartItemDto.Quantity == 0)
                {
                    if (existingCartItem != null)
                    {
                        _context.CartItems.Remove(existingCartItem);
                        await _context.SaveChangesAsync();
                        return Ok(new { Message = "Item removed from cart." });
                    }
                    return NotFound(new { Message = "Item not found in cart." });
                }

                // If the item already exists, update its quantity and price
                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += cartItemDto.Quantity;  
                     // Update the price if needed
                    _context.CartItems.Update(existingCartItem);
                }
                else
                {
                    // If the item does not exist, add it as a new item in the cart
                    var newCartItem = new CartItem
                    {
                        CartId = existingCart.Id,
                        ItemId = cartItemDto.ItemId,
                        Quantity = cartItemDto.Quantity,
                        price = cartItemDto.Price
                    };
                    _context.CartItems.Add(newCartItem);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Item added/updated in cart." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding/updating the item in the cart.");
                return StatusCode(500, new { Message = "An error occurred while adding/updating the item in the cart." });
            }
        }

        [HttpPut("updateCartitem/quantity")]
       // [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateCartItemQuantity([FromBody] CartItemDto CartItemDto)
        {
            try
            {
                var userId = GetUserId(); // Retrieve the authenticated user's ID

                // Retrieve the user's existing cart
                var userCart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (userCart == null)
                {
                    return NotFound(new { Message = "No cart found for the user." });
                }

                // Find the cart item to update
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == userCart.Id && ci.ItemId == CartItemDto.ItemId);

                if (cartItem == null)
                {
                    return NotFound(new { Message = "Item not found in the cart." });
                }

               
                cartItem.Quantity += CartItemDto.Quantity;

                // If the updated quantity is less than or equal to zero, remove the item from the cart
                if (cartItem.Quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    _context.CartItems.Update(cartItem); // Update the cart item
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Cart item quantity updated successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception here if you have a logging service
                _logger.LogError(ex, "An error occurred while updating the cart item quantity.");
                return StatusCode(500, new { Message = "An error occurred while updating the cart item quantity." });
            }
        }

    }
}
