using ECommerce_Final_Demo.Models;
using ECommerce_Final_Demo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace ECommerce_Final_Demo.Controllers
{
    public class OrderController : Controller
    {
        private readonly string _baseUrl = "https://localhost:7171/api/";
        private readonly IHttpClientFactory _httpClientFactory;
        public OrderController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

        }

        public async Task<IActionResult> ListOrders()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Order/ListOrders";  // The API endpoint

            try
            {
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Deserialize into a list of orders
                    var orders = JsonSerializer.Deserialize<List<Order>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    var ItemViewModels = orders.Select(Order.ToViewModel).ToList();

                    return View(ItemViewModels);  // Pass the list of orders to the view
                }
                else
                {
                    ModelState.AddModelError("", "Failed to load orders.");
                    return View(new List<OrderViewModel>());
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", "Network error: " + ex.Message);
                return View(new List<OrderViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {

            var storeId = GetStoreIdFromToken();

            if (string.IsNullOrEmpty(storeId))
            {

                ViewBag.ErrorMessage = "StoreId is not available in the token.";
                return View("Error");
            }


            var token = HttpContext.Session.GetString("UserSession");


            var client = _httpClientFactory.CreateClient();

            var StoreId = new
            {
                StoreId = storeId
            };

            var content = new StringContent(JsonSerializer.Serialize(StoreId), Encoding.UTF8, "application/json");


            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var orderurl = $"{_baseUrl}Order/placeorder";

            try
            {
                // Send the POST request to the API
                var response = await client.PostAsync(orderurl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    // Handle success
                    ViewBag.Message = "Order placed successfully!";
                    return View(); // Redirect or display success message
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    // Handle error
                    ViewBag.ErrorMessage = "Oops! cart is empty" + errorMessage;
                    return View(); 
                }
            }
            catch (HttpRequestException e)
            {
                // Handle network errors
                ViewBag.ErrorMessage = "Network error: " + e.Message;
                return View("Error");
            }
        }


        private string GetStoreIdFromToken()
        {
            // Retrieve token from session
            var token = HttpContext.Session.GetString("UserSession");

            if (string.IsNullOrEmpty(token))
            {
                return null; // No token available
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Extract StoreId from token claims
            var storeIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "StoreId");

            return storeIdClaim?.Value; // Return StoreId or null
        }

        public async Task<IActionResult> AcceptOrder(Guid orderId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Order/accept/{orderId}";  // The API endpoint

            try
            {
                // Send a POST request to accept the order
                var response = await httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response to get the message and bill
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AcceptOrderResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    // Create a ViewModel to pass to the view
                    var viewModel = new AcceptOrderResponse
                    {
                        Message = result.Message,
                        Bill = result.Bill
                    };

                    return View( viewModel);
                }
                else
                {
                    ModelState.AddModelError("", "Failed to accept the order.");
                    return View("Error");
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", "Network error: " + ex.Message);
                return View("Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteOrder(Guid orderId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}OrderApi/delete/{orderId}";

            try
            {
                var response = await httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // Optionally handle the response
                    ViewBag.Message = "Order deleted successfully.";
                    return RedirectToAction("Index"); // Redirect to a list or index view
                }
                else
                {
                    ViewBag.Message = "Failed to delete the order.";
                    return View("Error"); // Handle error view or message
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred: {ex.Message}";
                return View("Error"); // Handle error view or message
            }
        }
    }
}