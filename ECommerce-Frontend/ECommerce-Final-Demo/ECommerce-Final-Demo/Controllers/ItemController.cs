using ECommerce_Final_Demo.FileUpload;
using ECommerce_Final_Demo.Models;
using ECommerce_Final_Demo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ECommerce_Final_Demo.Controllers
{
    public class ItemController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly FileUploadService _fileUploadService;
        private readonly string _baseUrl = "https://localhost:7171/api/";
        private readonly string _imageUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

        public ItemController(IHttpClientFactory httpClientFactory, FileUploadService fileUploadService)
        {
            _httpClientFactory = httpClientFactory;
            _fileUploadService = fileUploadService;
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemViewModel ItemViewModel)

        {

            if (ModelState.IsValid)
            {
                string imagePath = null;

                if (ItemViewModel.ImageFile != null && ItemViewModel.ImageFile.Length > 0)
                {
                    // Ensure the image upload directory exists
                    imagePath = await _fileUploadService.UploadFileAsync(ItemViewModel.ImageFile, _imageUploadPath);

                }
                var ItemData = new
                {
                    ItemViewModel.Name,
                    ItemViewModel.Category,
                    ItemViewModel.Price,
                    ItemViewModel.StoreId,
                    Image = imagePath
                };
                var json = JsonSerializer.Serialize(ItemData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = _httpClientFactory.CreateClient();

                var url = $"{_baseUrl}Item/additem";
                SetAuthorizationHeader(httpClient);
                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(StoreItems));
                }

                // Add more detailed error information
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $" Server response: {errorMessage}");
            }
            return View();
        }
        public async Task<IActionResult> StoreItems()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Item/allItem";
            SetAuthorizationHeader(httpClient);
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var Items = JsonSerializer.Deserialize<List<Item>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Use camelCase to match JSON property names
                });
                var ItemViewModels = Items.Select(Item.ToViewModel).ToList();
                return View(ItemViewModels);

                
            }
            return Json(new { error = "An error occurred while processing your request." });
        }

        public async Task<IActionResult> Details(Guid Id)
        {
            var httpClient = _httpClientFactory.CreateClient();


            var url = $"{_baseUrl}Item/getdetailsbyid{Id}";
            SetAuthorizationHeader(httpClient);
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var Item = JsonSerializer.Deserialize<Item>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Pass the user data to the view
                return View(Item);
            }

            // Handle the case where the API call was not successful
            ModelState.AddModelError("", "Unable to load user data. Please try again.");
            return View(new UserViewModel());
        }

        public async Task<IActionResult> Delete(Guid Id)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var url = $"{_baseUrl}Item/deleteitem/{Id}"; // The API endpoint to delete the user
            SetAuthorizationHeader(httpClient);
            var response = await httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("StoreItems"); // Redirect back to the user list after deletion
            }

            ModelState.AddModelError("", "Unable to delete user. Please try again.");
            return View("Error"); // Redirect to an error page or handle as needed
        }


        public async Task<IActionResult> Edit(Guid Id)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var url = $"{_baseUrl}Item/getdetailsbyid/{Id}";
            SetAuthorizationHeader(httpClient);
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadFromJsonAsync<ItemViewModel>();
                if (item != null)
                {
                    return View(item);
                }
                else
                {
                    ModelState.AddModelError("", "Item not found.");
                    return View("Error");
                }
            }

            ModelState.AddModelError("", "Unable to load user details. Please try again.");
            return View("Error");
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ItemViewModel ItemViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(ItemViewModel);
            }

            // Handle image upload
            string imagePath = ItemViewModel.ImageFile != null ?
                await _fileUploadService.UploadFileAsync(ItemViewModel.ImageFile, _imageUploadPath) :
                ItemViewModel.Image; // Assuming ExistingImagePath holds the current image name

            var itemData = new
            {
                ItemViewModel.Id,
                ItemViewModel.Name,
                ItemViewModel.Category,
                ItemViewModel.Price,
                ItemViewModel.StoreId,
                Image = imagePath // Use the new image or the existing one
            };
            var httpClient = _httpClientFactory.CreateClient();

            var url = $"{_baseUrl}Item/updateitem/{ItemViewModel.Id}";
            SetAuthorizationHeader(httpClient);
            var jsonContent = JsonSerializer.Serialize(itemData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("StoreItems");
            }

            // Add more detailed error information
            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Unable to update Item. Server response: {errorMessage}");
            return View(ItemViewModel);
        }

        //add to cart button here
        public async Task<IActionResult> GetStoreItems()
        {

            var storeId = GetStoreIdFromToken();
            if (string.IsNullOrEmpty(storeId))
            {
                return Unauthorized("StoreId not found in token.");
            }


            var httpClient = _httpClientFactory.CreateClient();

            var url = $"{_baseUrl}Item/listofitem?storeId={storeId}"; // Corrected API URL
            SetAuthorizationHeader(httpClient);
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var Items = JsonSerializer.Deserialize<List<Item>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Use camelCase to match JSON property names
                });
                var ItemViewModels = Items.Select(Item.ToViewModel).ToList();

                var token = HttpContext.Session.GetString("UserSession");

                SetAuthorizationHeader(httpClient);
                var getCartItemsUrl = $"{_baseUrl}Cart/items";

                response = await httpClient.GetAsync(getCartItemsUrl);

                if (response.IsSuccessStatusCode)
                {

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var cartItems = JsonSerializer.Deserialize<List<CartItemViewModel>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Use camelCase to match JSON property names
                    });

                    ViewBag.cartItems = cartItems;
                }
               
                return View(ItemViewModels);


            }
            else
            {
                // Handle API call failure
                ModelState.AddModelError("", "Failed to load store items.");
                return View(); // Return the view with the error
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