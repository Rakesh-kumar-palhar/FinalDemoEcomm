using ECommerce_Final_Demo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<JsonResult> AddToCart(Guid id, decimal price, int quantity)
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

            // Send the POST request
            var response = await client.PostAsync(addItemUrl, content);

            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                return Json(new { success = false, error = response.ReasonPhrase });
            }
            return Json(new { success = true, succes = response.ReasonPhrase });
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
                ViewBag.ErrorMessage = "Error fetching cart items.";
                return View("Error");
            }

            // Deserialize the JSON response to a list of CartItemViewModel
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var cartItems = JsonSerializer.Deserialize<List<CartItemViewModel>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (cartItems == null || !cartItems.Any())
            {
                // If there are no items in the cart, show the empty cart view
                ViewBag.EmptyCartMessage = "Your cart is empty.";
                return View("CartEmpty"); 
            }

            // If there are items, show the cart items view
            return View(cartItems);
        }

        [HttpPost]
        public async Task<JsonResult> RemoveFromCart(Guid itemId)
        {
            var token = HttpContext.Session.GetString("UserSession");

            var client = _httpClientFactory.CreateClient();

            // Set the authorization header
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Define the URL for removing the item
            var removeItemUrl = $"{_baseUrl}Cart/removeitem/{itemId}";

            // Send the DELETE request
            var response = await client.DeleteAsync(removeItemUrl);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = response.ReasonPhrase });
            }

        }
    }
}
