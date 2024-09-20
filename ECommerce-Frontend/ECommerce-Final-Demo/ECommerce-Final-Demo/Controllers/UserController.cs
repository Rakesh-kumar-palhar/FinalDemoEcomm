using ECommerce_Final_Demo.FileUpload;
using ECommerce_Final_Demo.Models;
using ECommerce_Final_Demo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using ECommerce_Final_Demo.FileUpload;
using NuGet.Common;
using System.Net.Http.Headers;

namespace ECommerce_Final_Demo.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl = "https://localhost:7171/api/";
        private readonly string _imageUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        private readonly FileUploadService _fileUploadService;
       

        public UserController(IHttpClientFactory httpClientFactory, FileUploadService fileUploadService)
        {
            _httpClientFactory = httpClientFactory;
            _fileUploadService = fileUploadService;
        }

        public async Task<IActionResult> StoreUsers()
        {
            var httpClient = _httpClientFactory.CreateClient();

            var url = $"{_baseUrl}User/allusers"; 
            SetAuthorizationHeader(httpClient);

            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var users = JsonSerializer.Deserialize<List<User>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                var userViewModels = new List<UserViewModel>();
                foreach (var user in users)
                {
                    var userViewModel = new UserViewModel
                    {
                        Id = user.Id,
                        FName = user.FName,
                        LName = user.LName,
                        Email = user.Email,
                        MobileNumber = user.MobileNumber,
                        Role = user.Role,
                        CreateDate = user.CreateDate,
                        UpdateDate = user.UpdateDate,
                        IsActive = user.IsActive,
                        Profile = user.Profile,
                        StoreId = user.StoreId,
                        StoreName= user.StoreName
                        
                    };
                    userViewModels.Add(userViewModel);
                }

                return View(userViewModels);
                
                
            }
            return Json(new { error = "An error occurred while processing your request." });
        }


        public async Task<IActionResult> Details(Guid Id)
        {
            var httpClient = _httpClientFactory.CreateClient();

            SetAuthorizationHeader(httpClient);
            var url = $"{_baseUrl}User/getbyid{Id}";

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();


                var user = JsonSerializer.Deserialize<User>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });

                // Pass the user data to the view
                return View(user);
            }

            // Handle the case where the API call was not successful
            ModelState.AddModelError("", "Unable to load user data. Please try again.");
            return View(new UserViewModel());
        }

        public async Task<IActionResult> Delete(Guid Id)
        {
            var httpClient = _httpClientFactory.CreateClient();

            SetAuthorizationHeader(httpClient);
            var url = $"{_baseUrl}User/deleteuser{Id}"; 

            var response = await httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
              
                return RedirectToAction("StoreUsers");
            }

            ModelState.AddModelError("", "Unable to delete user. Please try again.");
            return View("Error"); 
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid Id)
        {
            var httpClient = _httpClientFactory.CreateClient();
            SetAuthorizationHeader(httpClient);
            var url = $"{_baseUrl}User/getbyid{Id}";

            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserViewModel>();
                return View(user);
            }

            ModelState.AddModelError("", "Unable to load user details. Please try again.");
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel userViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(userViewModel);
            }

            // Handle image upload
            string imagePath = userViewModel.ProfileImage != null
                ? await _fileUploadService.UploadFileAsync(userViewModel.ProfileImage, _imageUploadPath)
                : userViewModel.Profile; // Assuming Image holds the current image name

            var userData = new
            {
                userViewModel.Id,
                userViewModel.FName,
                userViewModel.LName,
                userViewModel.Email,
                userViewModel.MobileNumber,
                userViewModel.Role,
                userViewModel.CreateDate,
                userViewModel.UpdateDate,
                userViewModel.IsActive,
                userViewModel.Password,
                userViewModel.StoreId,

                Profile = imagePath // Use the new image or the existing one
            };

            var httpClient = _httpClientFactory.CreateClient();
            SetAuthorizationHeader(httpClient);
            var url = $"{_baseUrl}User/UpdateUser/{userViewModel.Id}";

            var jsonContent = JsonSerializer.Serialize(userData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("StoreUsers");
            }

            // Add more detailed error information
            var errorMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Unable to update user. Server response: {errorMessage}");
            return View(userViewModel);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel userViewModel)

        {

            if (ModelState.IsValid)
            {
                string imagePath = null;

                if (userViewModel.ProfileImage != null && userViewModel.ProfileImage.Length > 0)
                {
                    // Ensure the image upload directory exists
                    imagePath = await _fileUploadService.UploadFileAsync(userViewModel.ProfileImage, _imageUploadPath);

                }
                var userData = new
                {
                    userViewModel.FName,
                    userViewModel.LName,
                    userViewModel.Email,
                    userViewModel.Password,
                    userViewModel.MobileNumber,
                    userViewModel.Role,
                    userViewModel.CreateDate,
                    userViewModel.UpdateDate,
                    userViewModel.IsActive,
                    userViewModel.StoreId,
                    Profile = imagePath // Profile will store the file name (not full path)
                };
                var json = JsonSerializer.Serialize(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = _httpClientFactory.CreateClient();
                SetAuthorizationHeader(httpClient);
                var url = $"{_baseUrl}User/createuser";

                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(StoreUsers));
                }

                // Add more detailed error information
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Unable to create user. Server response: {errorMessage}");


            }
            return View(userViewModel);
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