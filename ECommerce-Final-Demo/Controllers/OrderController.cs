using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using ECommerce_Final_Demo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILoggerService _logger;
        public OrderController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILoggerService logger)
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
        
        [HttpPost("placeorder")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderDto orderDto)
        {
            try
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

                
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();

                return Ok("orderplaced succesfull");
                
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                return StatusCode(500, new { Message = "An error occurred while place order." });
            }
        }
        
        [HttpGet("ListOrders")]
        [Authorize(Roles = "SuperAdmin,StoreAdmin")]
        public async Task<IActionResult> ListOrders()
        {
            try {
                
                var pendingOrders = await _context.Orders
                    .Where(o => o.Status == false)  
                    .Include(o => o.User)           
                    .Include(o => o.Store)
                    .Select(o => new OrderDto
                    {
                        OrderId = o.OrderId,
                        UserId = o.UserId,
                        UserName = o.User.FName + " " + o.User.LName,
                        StoreId = o.StoreId,
                        StoreName = o.Store.Name,
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                    })
                    .ToListAsync();

                return Ok(pendingOrders);
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                return StatusCode(500, new { Message = "An error occurred while accept order." });
            }
        }

        [HttpPost("accept/{orderId}")]
        [Authorize(Roles = "SuperAdmin,StoreAdmin")]
        public async Task<IActionResult> AcceptOrder(Guid orderId)
        {
            try {
                var order = await _context.Orders.Include(o => o.OrderItems)
                    .Include(o => o.Store)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                
                if (order == null)
                {
                    return NotFound("Order not found.");
                }
                order.Status = true;
                var bill = GenerateBill(order);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Order accepted and bill generated.", Bill = bill });
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                return StatusCode(500, new { Message = "An error occurred while delete order." });
            }
        }
        private object GenerateBill(Order order)
        {
            return new
            {
                OrderId = order.OrderId,
                UserName = $"{order.User.FName} {order.User.LName}",
                StoreName = order.Store.Name,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount
            };
        }

        [HttpDelete("deleteorder/{orderId}")]
        [Authorize(Roles = "SuperAdmin,StoreAdmin")]
        public async Task<IActionResult> DeleteOrder(Guid orderId)
        {
            try {

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
            catch (Exception ex)
            {
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while delete order." });
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