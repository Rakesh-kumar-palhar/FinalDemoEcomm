using ECommerce_Final_Demo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ECommerce_Final_Demo.Controllers
{
    public class CartController : Controller
    {
        private readonly string _baseUrl = "https://localhost:7171/api/";
        private readonly IHttpClientFactory _httpClientFactory;

        public CartController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid id, decimal price, int quantity)
        {
            var token = HttpContext.Session.GetString("UserSession");

            // Create the HttpClient instance
            var client = _httpClientFactory.CreateClient();

            // Prepare the request payload
            var cartData = new
            {
                ItemId = id,
                Price = price,
                Quantity = quantity
            };

            var content = new StringContent(JsonSerializer.Serialize(cartData), Encoding.UTF8, "application/json");

            // Set the authorization header
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Define the URL for adding the item
            var addItemUrl = $"{_baseUrl}Cart/additem";

            try
            {               
                var response = await client.PostAsync(addItemUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    
                    TempData["SuccessMessage"] = "Item added to cart successfully!";
                }
                else
                {

                    TempData["ErrorMessage"] = "Error adding item to cart: "; 
                }
            }
            catch (HttpRequestException e)
            {
                
                TempData["ErrorMessage"] = "Network error: " + e.Message;
            }

            return RedirectToAction("GetStoreItems", "Item"); // Redirect to an appropriate view or page
        }
        public async Task<IActionResult> CartItems()
        {
            var token = HttpContext.Session.GetString("UserSession");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var getCartItemsUrl = $"{_baseUrl}Cart/items";

            var response = await client.GetAsync(getCartItemsUrl);

            if (!response.IsSuccessStatusCode)
            {
                // If the API call fails, show the error view
                ViewBag.ErrorMessage = "Cart Is Empty";
                return View();
            }

            // Deserialize the JSON response to a list of CartItemViewModel
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var cartItems = JsonSerializer.Deserialize<List<CartItemViewModel>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

           

            // If there are items, show the cart items view
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid itemId)
        {
            var token = HttpContext.Session.GetString("UserSession");

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            
            var removeItemUrl = $"{_baseUrl}Cart/removeitem/{itemId}";
           
            
            var response = await client.DeleteAsync(removeItemUrl);

            if (response.IsSuccessStatusCode)
            {
               
                TempData["SuccessMessage"] = "Item removed from cart successfully.";
            }
            else
            {
               
                TempData["ErrorMessage"] = "Error removing item from cart: " + response.ReasonPhrase;
            }

            // Redirect to the CartItems action to refresh the cart view
            return RedirectToAction("CartItems", "Cart");
        }
       
    }
}
