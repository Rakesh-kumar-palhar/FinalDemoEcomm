using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ECommerce_Final_Demo.Controllers
{
   
    public class UserDeshboard : Controller
    {
        [Authorize]
        public IActionResult Deshboard()
        {
            var token = HttpContext.Session.GetString("UserSession");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Extract role claim
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            ViewBag.Role = roleClaim;
            return View();
        }
    }
}
