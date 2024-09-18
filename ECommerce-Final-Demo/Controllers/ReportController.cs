using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/[controller]")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReportController(ApplicationDbContext context)
        {
            _context = context;
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
                await LogException(ex);
                return StatusCode(500, new { Message = "An error occurred while GetDayByDayPurchaseReport generate." });
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
