using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   //[Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OrderController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
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
        [HttpPost("placeorder")]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderDto orderDto)
        {
            var userId = GetUserId();
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound("Cart not found for this user.");
            }

            var store = await _context.Stores.FindAsync(orderDto.StoreId);
            if (store == null)
            {
                return NotFound("Store not found.");
            }

            var orderItems = _context.CartItems.Where(ci => ci.CartId == cart.Id)
            .Select(ci => new OrderItem
            {
               
                ItemId = ci.ItemId,
                Quantity = ci.Quantity,
                Price = ci.price
            }).ToList();

            

            var totalAmount = orderItems.Sum(oi => oi.Price * oi.Quantity);
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                UserId = userId,
                StoreId = orderDto.StoreId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount

            };
            foreach (var orderItem in orderItems)
            {
                orderItem.OrderId = order.OrderId;
            }
            _context.Orders.Add(order);
            _context.OrderItems.AddRange(orderItems);

            // Remove items from the cart after placing the order
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return Ok("orderplaced succesfull");
            //new { OrderId = order.OrderId, TotalAmount = order.TotalAmount }
        }

        [HttpPost("accept/{orderId}")]
        public async Task<IActionResult> AcceptOrder(Guid orderId)
        {
            var order = await _context.Orders

                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound("Order not found.");
            }
            var bill = GenerateBill(order);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order accepted and bill generated.", Bill = bill });
        }
        
         private string GenerateBill(Order order)
         {        
          var billDetails = $"Bill for Order ID: {order.OrderId}\n" +
                          $"User ID: {order.UserId}\n" +
                          $"Store ID: {order.StoreId}\n" +
                          $"Order Date: {order.OrderDate}\n" +
                          $"Total Amount:{order.TotalAmount}\n";       
           
          billDetails += $"Total Amount: {order.TotalAmount}\n";
          return billDetails;
        }
        [HttpDelete("deleteorder/{orderId}")]
        public async Task<IActionResult> DeleteOrder(Guid orderId)
        {
           
               
                var order = await _context.Orders                    
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return NotFound(new { Message = "Order not found." });
                }                              
                _context.Orders.Remove(order);               
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Order deleted successfully." });
            }
    }
}