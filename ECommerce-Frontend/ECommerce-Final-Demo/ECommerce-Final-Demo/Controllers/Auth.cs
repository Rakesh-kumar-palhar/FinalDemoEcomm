﻿using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using ECommerce_Final_Demo.Models.ViewModels;
using ECommerce_Final_Demo.FileUpload;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ECommerce_Final_Demo.Models;
using Microsoft.AspNetCore.Identity;


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
                    TempData["SuccessMessage"] = "Registration successful! Please log in.";
                    return RedirectToAction("Login");
                }
                else
                {

                    TempData["ErrorMessage"] = "Registration failed. User already exists with this email.";

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
                    var nameid = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                    // Create claims for the cookie
                    var claims = new List<Claim>
                          {
                      new Claim(ClaimTypes.Name, model.Email),
                       
                          new Claim("Token", token),
                          new Claim("StoreId", StoreId.ToString()),
                          new Claim(ClaimTypes.Role,role.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, nameid .ToString())
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
                    TempData["SuccessMessage"] = "Login successful!";
                    return RedirectToAction("Deshboard", "UserDeshboard");
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        ModelState.AddModelError("", "Invalid User name and Password. Please try again.");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        ModelState.AddModelError("", "User not registered. Please sign up first.");
                    }

                }
               

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

                   
                    TempData["SuccessMessage"] = "Logout successful!";
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    // Handle failure response
                    TempData["SuccessMessage"] = "Logout Fail! Please log in.";
                    return View("Error");
                }
            }
            catch (HttpRequestException e)
            {
                // Handle network errors
                TempData["SuccessMessage"] = "Network error! Please log in.";
                return View("Error");
            }

        }
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{_baseUrl}Auth/change-password"; 

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var requestData = new
                {
                    UserId = userId,
                    currentPassword = model.CurrentPassword,
                    newPassword = model.NewPassword
                };

                var response = await httpClient.PostAsJsonAsync(url, requestData);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Password changed successfully.";
                    return RedirectToAction("Deshboard", "UserDeshboard");

                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = "Current password is Wrong";
                }
            }

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> IsEmailAvailable(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json("Email is required.");
            }
            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}Auth/EmailCheck?email={email}";
            var response = await httpClient.GetAsync(url);           
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                bool emailExists = bool.TryParse(content, out bool exists) && exists;
                if (emailExists)
                {
                    return Json($"{email} is already in use.");
                }
            }
            else
            {
                return Json("Error checking email availability."); 
            }

            return Json(true);
        }

    }

}


        
    
