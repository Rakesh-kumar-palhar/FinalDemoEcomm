using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
using ECommerce_Final_Demo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ECommerce_Final_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<UserController> _logger;
        public UserController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, ILogger<UserController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [HttpGet("alluser")]
       [Authorize(Roles = "SuperAdmin,StoreAdmin")]
        public async Task<IActionResult> GetUsers([FromQuery] Guid? storeId)
        {
            try {
                
                var users = storeId.HasValue
                    ? await _context.Users
                        .Where(u => u.StoreId == storeId.Value && u.IsActive == true)  
                        .ToListAsync()
                    : await _context.Users
                        .Where(u => u.IsActive == true)  
                        .ToListAsync();

                var userDtos = UserDto.Mapping(users);
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getuser.");
                return StatusCode(500, "An error occurred while getuser.");
            }
        }
        [HttpGet("allusers")]
        [Authorize(Roles = "SuperAdmin,StoreAdmin")]
        public async Task<IActionResult> GetUserss()
        {
            try {
                
                var users = await _context.Users
                    .Include(u => u.Store)              
                    .Where(u => u.Role == "User" && u.IsActive == true)  
                    .ToListAsync();                
                var userDtos = UserDto.Mapping(users);
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while get the users.");
                return StatusCode(500, new { Message = "An error occurred while get the users." });
            }
        }
        [Authorize(Roles = "SuperAdmin,StoreAdmin,User")]
        [HttpGet("getbyid{userId:guid}")]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            try {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                var userDto = UserDto.Mapping(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while get user by id.");
                return StatusCode(500, new { Message = "An error occurred while get user by id." });
            }
        }
        [HttpPost("createuser")]
        [Authorize(Roles = "SuperAdmin,StoreAdmin")]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
            try {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if the provided StoreId exists
                if (userDto.StoreId.HasValue)
                {
                    var storeExists = await _context.Stores.AnyAsync(s => s.Id == userDto.StoreId.Value);
                    if (!storeExists)
                    {
                        return BadRequest(new { Message = "Invalid StoreId provided." });
                    }
                }

                var user = UserDto.Mapping(userDto);

                // Hash the user's password before saving
                user.Password = _passwordHasher.HashPassword(user, userDto.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, UserDto.Mapping(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while create user.");
                return StatusCode(500, new { Message = "An error occurred while create user." });
            }
        }

        [HttpPut("UpdateUser/{userId:guid}")]
        [Authorize(Roles = "SuperAdmin,StoreAdmin,User")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UserDto userDto)
        {
            try {
                if (userId != userDto.Id)
                {
                    return BadRequest(new { Message = "User ID mismatch." });
                }

                var existingUser = await _context.Users.FindAsync(userId);
                if (existingUser == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                // Update user properties with the provided DTO values
                existingUser.FName = userDto.FName;
                existingUser.LName = userDto.LName;
                existingUser.Email = userDto.Email;
                existingUser.Password = userDto.Password;
                existingUser.MobileNumber = userDto.MobileNumber;
                existingUser.Role = userDto.Role;
                existingUser.UpdateDate = DateTime.UtcNow;
                existingUser.IsActive = userDto.IsActive;
                existingUser.Profile = userDto.Profile;
                existingUser.StoreId = userDto.StoreId;
                existingUser.UpdatedBy = userDto.updatedBy;
                existingUser.Token = userDto.Token;

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while update the user.");
                return StatusCode(500, new { Message = "An error occurred while update the user." });
            }
        }

        [HttpDelete("deleteuser{userId:guid}")]
        [Authorize(Roles = "SuperAdmin,StoreAdmin")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            try {
                // Fetch the user from the database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                // If user not found, return 404
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Mark the user as inactive (soft delete)
                user.IsActive = false;               
                user.UpdateDate = DateTime.UtcNow;

               
                await _context.SaveChangesAsync();

                // Return success response
                return Ok(new { Message = "User has been soft deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while delete the user.");
                return StatusCode(500, new { Message = "An error occurred while delete the user." });
            }
        }
       
    }
}
