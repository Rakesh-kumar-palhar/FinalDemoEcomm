using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        [HttpPost("create")]
        public async Task<IActionResult> CreateCart()
        {
            var userId = GetUserId();
            var existingCart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (existingCart != null)
            {
                return Conflict("Cart already exists for this user.");
            }

            var newCart = new Cart
            {

                UserId = userId
            };

            _context.Carts.Add(newCart);
            await _context.SaveChangesAsync();

            return Ok("cart createsuccesfull");
        }

        

        [HttpPost("additem")]
        public async Task<IActionResult> AddItemToCart(Guid CartId,[FromBody]CartItemDto cartitemdto)
        {
            var existingCartItem = await _context.CartItems
        .FirstOrDefaultAsync(ci => ci.CartId == CartId && ci.ItemId == cartitemdto.ItemId);

            if (cartitemdto.Quantity == 0)
            {
                if (existingCartItem != null)
                {
                    // Remove item from the cart if quantity is 0
                    _context.CartItems.Remove(existingCartItem);
                    await _context.SaveChangesAsync();
                    return Ok("Item removed from cart.");
                }
                return NotFound("Item not found in cart.");
            }
            if (existingCartItem != null)
            {
                // Update existing item quantity and price
                existingCartItem.Quantity = cartitemdto.Quantity;
                existingCartItem.price = cartitemdto.Price;
                _context.CartItems.Update(existingCartItem);
            }
            else
            {

                var cartItem = new CartItem()
                {
                    CartId = CartId,
                    ItemId = cartitemdto.ItemId,
                    Quantity = cartitemdto.Quantity,
                    price = cartitemdto.Price

                };

                _context.CartItems.Add(cartItem);
            }
                await _context.SaveChangesAsync();

                return Ok("Item added/updated in cart.");
            }
        
        
    }
}
