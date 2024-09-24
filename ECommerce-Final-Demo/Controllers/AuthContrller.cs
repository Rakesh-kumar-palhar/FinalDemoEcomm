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
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenServices _jwtTokenServices;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger <AuthController>_logger;
        public AuthController(ApplicationDbContext context, JwtTokenServices jwtTokenServices, IPasswordHasher<User> passwordHasher, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtTokenServices = jwtTokenServices;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var existingUser = await _context.Users
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (model.Email == "trigger500@example.com")
                {
                    throw new InvalidOperationException("This is a test to trigger a 500 error.");
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration for email: {Email}", model.Email);  // Logging the exception with relevant details
                return StatusCode(500, new { Message = "An error occurred during registration." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                {
                    return Unauthorized(new { Message = "Invalid email or password." });
                }
                if (!user.IsActive)
                {
                    return Unauthorized(new { Message = "Your account is inactive. Please contact support." });
                }

                if (user == null || _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password) == PasswordVerificationResult.Failed)
                {
                    return Unauthorized(new { Message = "Invalid email or password." });
                }

                var token = _jwtTokenServices.GenerateToken(user.Id, user.Role, user.StoreId);

                user.Token = token;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for email: {Email}", model.Email);  // Logging the exception with relevant details
                return StatusCode(500, new { Message = "An error occurred during Login." });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                {
                    return Unauthorized(new { Message = "User is not authenticated." });
                }

                var userId = Guid.Parse(userIdClaim);
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                user.Token = null;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "User logged out successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout for email: {Email}");  // Logging the exception with relevant details
                return StatusCode(500, new { Message = "An error occurred during logout." });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword model)
        {
            try
            {
                // Fetch the user from the database
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                // Validate the current password
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, model.CurrentPassword);
                if (passwordVerificationResult == PasswordVerificationResult.Failed)
                {
                    return BadRequest(new { Message = "Current password is incorrect." });
                }

                // Hash the new password
                user.Password = _passwordHasher.HashPassword(user, model.NewPassword);

                // Update the user in the database
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during change for password");  // Logging the exception with relevant details
                return StatusCode(500, new { Message = "An error occurred during change password." });
            }
        }
    }
}
