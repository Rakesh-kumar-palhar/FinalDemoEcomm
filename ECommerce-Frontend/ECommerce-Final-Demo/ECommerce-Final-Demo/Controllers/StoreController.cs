using ECommerce_Final_Demo.FileUpload;
using ECommerce_Final_Demo.Helper;
using ECommerce_Final_Demo.Models;
using ECommerce_Final_Demo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using static ECommerce_Final_Demo.Helper.StoreLocation;

namespace ECommerce_Final_Demo.Controllers
{
    public class StoreController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl = "https://localhost:7171/api/";
        private readonly string _imageUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        private readonly FileUploadService _fileUploadService;
        private readonly StoreLocation _storeLocation;
        

        public StoreController(IHttpClientFactory httpClientFactory, FileUploadService fileUploadService, 
            StoreLocation storeLocation)
        {
            _httpClientFactory = httpClientFactory;
            _fileUploadService = fileUploadService;
            _storeLocation = storeLocation;
            
        }
        public async Task<IActionResult> StoreList()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}stores/allstores";

            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var storeData = JsonSerializer.Deserialize<List<Store>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Use camelCase to match JSON property names
                });

                // Manually map the data
                var storeViewModels = new List<StoreViewModel>();
                foreach (var store in storeData)
                {
                    var storeViewModel = new StoreViewModel
                    {
                        Id = store.Id,
                        Name = store.Name,
                        Country = await _storeLocation.GetCountryNameAsync(store.CountryId), // Fetch country name
                        State = await _storeLocation.GetStateNameAsync(store.StateId),     // Fetch state name
                        City = await _storeLocation.GetCityNameAsync(store.CityId),
                        Image = store.Image
                    };

                    storeViewModels.Add(storeViewModel);
                }
                return View(storeViewModels);
            }
            else
            {                
                return View("Error"); 
            }
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(StoreViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                string imagePath = null;

                
                if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                {
                    
                    try
                    {
                        imagePath = await _fileUploadService.UploadFileAsync(viewModel.ImageFile, _imageUploadPath);
                    }
                    catch (Exception ex)
                    {
                        
                        ModelState.AddModelError(string.Empty, $"File upload failed: {ex.Message}");
                        return View(viewModel);
                    }
                }
                else
                {
                  
                    imagePath = null;
                }
                var StoreData = new
                {
                    viewModel.Name,
                    viewModel.CountryId,
                    viewModel.StateId,
                    viewModel.CityId,
                    Image = imagePath 
                };
                var json = JsonSerializer.Serialize(StoreData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{_baseUrl}stores/addstore";

                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(StoreList));
                }
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid Id)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}stores/storedetails{Id}";

            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<StoreViewModel>();
                return View(user);
            }

            ModelState.AddModelError("", "Unable to load user details. Please try again.");
            return View("Error");
        }


        [HttpPost]
        public async Task<IActionResult> Edit(StoreViewModel storeViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(storeViewModel);
            }

            // Handle image upload
            string imagePath = storeViewModel.ImageFile != null
                ? await _fileUploadService.UploadFileAsync(storeViewModel.ImageFile, _imageUploadPath)
                : storeViewModel.Image; // Assuming Image holds the current image name


            var storeData = new
            {
                storeViewModel.Id,
                storeViewModel.Name,
                storeViewModel.CountryId,
                storeViewModel.StateId,
                storeViewModel.CityId,
                

                Image = imagePath // Use the new image or the existing one
            };

            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}stores/updatestore{storeViewModel.Id}";

            var jsonContent = JsonSerializer.Serialize(storeData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("StoreList");
            }

            // Add more detailed error information
            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Unable to update user. Server response: {errorMessage}");
            return View(storeViewModel);
        }

        public async Task<IActionResult> Delete(Guid Id)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}stores/{Id}"; // The API endpoint to delete the user

            var response = await httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("StoreList"); // Redirect back to the user list after deletion
            }

            ModelState.AddModelError("", "Unable to delete user. Please try again.");
            return View("Error"); // Redirect to an error page or handle as needed
        }

    }
}