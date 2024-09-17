using ECommerce_Final_Demo.Models;
using ECommerce_Final_Demo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
namespace ECommerce_Final_Demo.Controllers
{
      
    public class ReportController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl = "https://localhost:7171/api/";
        private readonly HttpClient _httpClient;

        public ReportController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> DayByDayPurchase()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Report/DayByDayPurchase";

            try
            {
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<DayByDayPurchaseReportViewModel>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Use camelCase to match JSON property names
                    });

                    return View(items);
                }
                else
                {
                    // Handle response errors (e.g., log or show an error message)
                    return View("Error"); // You can create an Error view to handle this case
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                return View("Error"); // You can create an Error view to handle this case
            }
        }
    }
}
