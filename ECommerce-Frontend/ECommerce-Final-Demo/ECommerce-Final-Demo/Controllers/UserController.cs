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
using System.Security.Claims;

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
                        FirstName = user.FName,
                        LastName = user.LName,
                        Email = user.Email,
                        MobileNumber = user.MobileNumber,
                        Role = user.Role,
                        CreateDate = user.CreateDate,
                        UpdateDate = user.UpdateDate,
                        IsActive = user.IsActive,
                        Profile = user.Profile,
                        StoreId = user.StoreId,
                        StoreName = user.StoreName,
                        Password = user.Password
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
            var sessionUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (sessionUserId == null || sessionUserId != Id.ToString())
            {

                ViewData["IsCreatingUser"] = true;

            }
            else
            {
                ViewData["IsCreatingUser"] = false;
                
            }
            var httpClient = _httpClientFactory.CreateClient();
            SetAuthorizationHeader(httpClient);
            var url = $"{_baseUrl}User/getbyid{Id}";

            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<User>();
                var userViewModel = UserViewModel.ToViewModel(user);
                
                return View(userViewModel);
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
            var updatedBy = User.FindFirst(ClaimTypes.Role)?.Value;
            var userData = new
            {
                userViewModel.Id,
                FName = userViewModel.FirstName, 
                LName = userViewModel.LastName,  
                userViewModel.Email,
                userViewModel.MobileNumber,
                userViewModel.Role,
                userViewModel.CreateDate,
                userViewModel.UpdateDate,
                userViewModel.IsActive,
                userViewModel.Password,
                userViewModel.StoreId,
                updatedBy,
                Profile = imagePath 
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

        private static bool IsPasswordValid(string password)
        {
            // Check for null or empty
            if (string.IsNullOrEmpty(password))
                return false;

            // Check for minimum length
            if (password.Length < 8)
                return false;

            // Check for at least one uppercase letter, one lowercase letter, one number, and one special character
            bool hasUpperCase = false;
            bool hasLowerCase = false;
            bool hasDigit = false;
            bool hasSpecialChar = false;

            // Define a set of special characters to check against
            string specialChars = "@#$&*";

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpperCase = true;
                if (char.IsLower(c)) hasLowerCase = true;
                if (char.IsDigit(c)) hasDigit = true;
                if (specialChars.Contains(c)) hasSpecialChar = true;
            }

            // Password is valid only if it contains at least one of each category
            return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }

        public IActionResult Create()
        {
            var Model = new UserViewModel { IsActive = true };

            return View(Model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel userViewModel)

        {

            if (ModelState.IsValid)
            {
                if (!IsPasswordValid(userViewModel.Password))
                {
                    ModelState.AddModelError("Password", "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character from @#$&*.");
                    return View(userViewModel); // Return the view with the validation error
                }
                string imagePath = null;

                if (userViewModel.ProfileImage != null && userViewModel.ProfileImage.Length > 0)
                {
                    // Ensure the image upload directory exists
                    imagePath = await _fileUploadService.UploadFileAsync(userViewModel.ProfileImage, _imageUploadPath);

                }
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userData = new
                {
                    id = userViewModel.Id, 
                    fName = userViewModel.FirstName,
                    lName = userViewModel.LastName,
                    email = userViewModel.Email,
                    password = userViewModel.Password,  
                    mobileNumber = userViewModel.MobileNumber,
                    role = userViewModel.Role,
                    createDate = userViewModel.CreateDate,
                    updateDate = userViewModel.UpdateDate,
                    isActive = userViewModel.IsActive,
                    profile = imagePath, // Pass the image file name or path (depending on your needs)
                    storeId = userViewModel.StoreId,
                    createdBy= currentUserRole
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
            ViewData["IsCreatingUser"] = true;
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