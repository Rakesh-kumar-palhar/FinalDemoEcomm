﻿using ECommerce_Final_Demo.Model;
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

                // Remove items from the cart after placing the order
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();

                return Ok("orderplaced succesfull");
                //new { OrderId = order.OrderId, TotalAmount = order.TotalAmount }
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while place order." });
            }
        }
        [HttpGet("ListOrders")]
        public async Task<IActionResult> ListOrders()
        {
            try {
                var orders = await _context.Orders
                    .Include(o => o.User)   // Load related User entity
                    .Include(o => o.Store)  // Load related Store entity
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

                return Ok(orders);
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while retrieving order." });
            }
        }
        [HttpPost("accept/{orderId}")]
        public async Task<IActionResult> AcceptOrder(Guid orderId)
        {
            try {
                var order = await _context.Orders
                    .Include(o => o.User)  // Load the related User entity
                    .Include(o => o.Store) // Load the related Store entity
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                var bill = GenerateBill(order);

                // Create a response object with order details
                var response = new
                {
                    OrderId = order.OrderId,
                    UserName = $"{order.User.FName} {order.User.LName}",
                    StoreName = order.Store.Name,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount
                };

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Order accepted and bill generated.", Bill = response });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while accept order." });
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