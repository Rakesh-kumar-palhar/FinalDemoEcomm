using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using ECommerce_Final_Demo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/[controller]")]
    //[Authorize(Roles = "SuperAdmin,StoreAdmin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService _logger;
        public ReportController(ApplicationDbContext context, ILoggerService logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("DayByDayPurchase")]
        public async Task<IActionResult> GetDayByDayPurchaseReport()
        {
            try {
                // Get orders with their items and store information
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Store)
                    .ToListAsync();

                // Group by date and store
                var report = orders
                    .GroupBy(o => new { o.OrderDate.Date, o.Store.Name })
                    .Select(g => new DayByDayPurchaseReportDto
                    {
                        Date = g.Key.Date,
                        StoreName = g.Key.Name,
                        TotalSales = g.Sum(o => o.OrderItems.Sum(oi => oi.Quantity * oi.Price)),
                        OrderItems = g.SelectMany(o => o.OrderItems.Select(oi => new OrderItemDto
                        {
                            ItemId = oi.ItemId,                           
                            Quantity = oi.Quantity,
                            Price = oi.Price,

                        })).ToList()
                    })
                    .OrderBy(r => r.Date)
                    .ToList();

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                return StatusCode(500, new { Message = "An error occurred while GetDayByDayPurchaseReport generate." });
            }

           
        }
        
    }
}
