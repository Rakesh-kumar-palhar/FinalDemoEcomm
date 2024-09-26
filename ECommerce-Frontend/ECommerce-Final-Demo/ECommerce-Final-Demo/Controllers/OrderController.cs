using ECommerce_Final_Demo.Models;
using ECommerce_Final_Demo.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
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
            var url = $"{_baseUrl}Order/ListOrders"; 
            SetAuthorizationHeader(httpClient);
            try
            {
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                   
                    var orders = JsonSerializer.Deserialize<List<Order>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    var ItemViewModels = orders.Select(Order.ToViewModel).ToList();
                    return View(ItemViewModels);  
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
               
                var response = await client.PostAsync(orderurl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    TempData["SuccessMessage"] = "Order placed successfully!";
                    return RedirectToAction("Deshboard", "UserDeshboard");
                }
                else
                {


                    TempData["errorMessage"] = "Oops ! your cart is empty";
                    return RedirectToAction("Deshboard", "UserDeshboard");
                }
            }
            catch (HttpRequestException e)
            {

                TempData["errorMessage"] = "Oops ! network error";
                return RedirectToAction("Deshboard", "UserDeshboard");
            }
        }


        private string GetStoreIdFromToken()
        {
            
            var token = HttpContext.Session.GetString("UserSession");

            if (string.IsNullOrEmpty(token))
            {
                return null; 
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

           
            var storeIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "StoreId");

            return storeIdClaim?.Value; 
        }

        public async Task<IActionResult> AcceptOrder(Guid orderId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Order/accept/{orderId}"; 
            SetAuthorizationHeader(httpClient);
            try
            {
                
                var response = await httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                   
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AcceptOrderResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    
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
            SetAuthorizationHeader(httpClient);
            try
            {
                var response = await httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                   
                    ViewBag.Message = "Order deleted successfully.";
                    return RedirectToAction("Index"); 
                }
                else
                {
                    ViewBag.Message = "Failed to delete the order.";
                    return View("Error"); 
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred: {ex.Message}";
                return View("Error"); 
            }
        }
        private void SetAuthorizationHeader(HttpClient httpClient)
        {
            var token = HttpContext.Session.GetString("UserSession");
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}