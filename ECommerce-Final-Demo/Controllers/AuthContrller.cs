using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthContrller : ControllerBase
    {
        
        private ApplicationDbContext _context;
        private readonly JwtTokenServices _jwtTokenServices;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthContrller(ApplicationDbContext context, JwtTokenServices jwtTokenServices, IPasswordHasher<User> passwordHasher)
        {

            _jwtTokenServices = jwtTokenServices;
            _passwordHasher = passwordHasher;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var existingUser = await _context.Users
           .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (existingUser != null)
            {
                return BadRequest(new { Message = "User with this email already exists." });
            }
            var user = new User
            {
                Id = Guid.NewGuid(),
                FName = model.FName,
                LName = model.LName,
                Email = model.Email,
                MobileNumber = model.MobileNumber,
                Role = "User",  // Default role
                CreateDate = DateTime.UtcNow,
                IsActive = true,
                Profile = model.Profile
            };

            user.Password = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password) == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            var token = _jwtTokenServices.GenerateToken(user.Id, user.Role,user.StoreId);

            user.Token = token;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { Token = token });
            

        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Get the user's ID from the token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized(new { Message = "User is not authenticated." });
            }

            // Parse the userId (assuming it is a Guid)
            var userId = Guid.Parse(userIdClaim);

            // Find the user by ID
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Clear the token
            user.Token = null;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User logged out successfully." });
        }

    }
}