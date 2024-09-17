using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using ECommerce_Final_Demo.Models.ViewModels;
using ECommerce_Final_Demo.FileUpload;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;


namespace ECommerce_Demo_Frontend.Controllers
{
    public class Auth : Controller
    {
        private readonly string _baseUrl = "https://localhost:7171/api/";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _imageUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

        private readonly FileUploadService _fileUploadService;

        public Auth(IHttpClientFactory httpClientFactory, FileUploadService fileUploadService)
        {
            _httpClientFactory = httpClientFactory;
            _fileUploadService = fileUploadService;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imagePath = null;

                if (model.Profile != null && model.Profile.Length > 0)
                {
                    // Ensure the image upload directory exists
                    imagePath = await _fileUploadService.UploadFileAsync(model.Profile, _imageUploadPath);

                }
                // Prepare the data for JSON
                var registrationData = new
                {
                    model.FName,
                    model.LName,
                    model.Email,
                    model.Password,
                    model.MobileNumber,
                    Profile = imagePath
                };

                var json = JsonSerializer.Serialize(registrationData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{_baseUrl}Auth/register"; // Append endpoint to the base URL

                // Send the request
                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    // Handle successful response, redirect to a success page                                   
                    return RedirectToAction("Login");
                }
                else
                {
                    // Handle error response, perhaps display the error message
                    ModelState.AddModelError("", "Registration failed. Please try again.");
                }
            }

            // If we got this far, something failed; redisplay the form with errors
            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var loginData = new
                {
                    model.Email,
                    model.Password
                };

                // Serialize using System.Text.Json
                var json = System.Text.Json.JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{_baseUrl}Auth/login";

                var response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
                    var token = jsonResponse["token"];



                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    var StoreId = jwtToken.Claims.FirstOrDefault(c => c.Type == "StoreId");
                    var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role").Value;

                    // Create claims for the cookie
                    var claims = new List<Claim>
                          {
                      new Claim(ClaimTypes.Name, model.Email),
                          new Claim("Token", token),
                          new Claim("StoreId", StoreId.ToString()),
                          new Claim(ClaimTypes.Role,role.ToString())
                          };


                    // Create claims identity for cookie authentication
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    //Set the authentication cookie
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true, // Make cookie persistent
                            ExpiresUtc = DateTime.UtcNow.AddMinutes(30) // Set expiration
                        });

                    HttpContext.Session.SetString("UserSession", token);
                    return RedirectToAction("Deshboard", "UserDeshboard");
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt. Please try again.");
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Session.GetString("UserSession");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var logoutUrl = $"{_baseUrl}Auth/logout";

            try
            {
                var response = await client.PostAsync(logoutUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    // Clear the session data
                    HttpContext.Session.Remove("UserSession");

                    // Redirect to login page or any other page
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    // Handle failure response
                    ViewBag.ErrorMessage = "Logout failed. Please try again.";
                    return View("Error");
                }
            }
            catch (HttpRequestException e)
            {
                // Handle network errors
                ViewBag.ErrorMessage = "Network error: " + e.Message;
                return View("Error");
            }

        }
    }

}


        
    
