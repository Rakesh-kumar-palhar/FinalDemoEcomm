using ECommerce_Final_Demo.FileUpload;
using ECommerce_Final_Demo.Models;
using ECommerce_Final_Demo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
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
                var userData = new
                {
                    ItemViewModel.Name,
                    ItemViewModel.Category,
                    ItemViewModel.Price,
                    ItemViewModel.StoreId,
                    Image = imagePath
                };
                var json = JsonSerializer.Serialize(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{_baseUrl}Item/additem";

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

            // API endpoint to fetch all users
            var url = $"{_baseUrl}Item/allItem"; // Adjust the endpoint as necessary

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

                //return View(users);
            }
            return Json(new { error = "An error occurred while processing your request." });
        }
        public async Task<IActionResult> Details(Guid Id)
        {
            var httpClient = _httpClientFactory.CreateClient();


            var url = $"{_baseUrl}Item/getdetailsbyid{Id}";
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
            var url = $"{_baseUrl}Item/deleteitem{Id}"; // The API endpoint to delete the user

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
            var url = $"{_baseUrl}Item/getdetailsbyid{Id}";

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
            var url = $"{_baseUrl}Item/updateitem{ItemViewModel.Id}";

            var jsonContent = JsonSerializer.Serialize(itemData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("StoreItems");
            }

            // Add more detailed error information
            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Unable to update user. Server response: {errorMessage}");
            return View(ItemViewModel);
        }

        public async Task<IActionResult> GetStoreItems()
        {
            // Step 1: Extract StoreId from token
            var storeId = GetStoreIdFromToken();
            if (string.IsNullOrEmpty(storeId))
            {
                return Unauthorized("StoreId not found in token.");
            }

            // Step 2: Call the API to get store-specific items
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Item/listofitem?storeId={storeId}"; // Corrected API URL

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
    }
}