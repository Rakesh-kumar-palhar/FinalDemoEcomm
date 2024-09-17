using ECommerce_Final_Demo.Model;
using ECommerce_Final_Demo.Model.DTO;
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
   // [Authorize(Roles = "SuperAdmin,StoreAdmin")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet("alluser")]
       // [Authorize(Roles = "SuperAdmin ")]
        public async Task<IActionResult> GetUsers([FromQuery] Guid? storeId)
        {

            var users = storeId.HasValue

                ? await _context.Users.Where(u => u.StoreId == storeId.Value).ToListAsync()
                : await _context.Users.ToListAsync();

            var userDtos = UserDto.Mapping(users);
            return Ok(userDtos);
        }

        [HttpGet("allusers")]
        // [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetUserss()
        {
            var users = await _context.Users
            .Include(u =>u.Store)
            .Where(u => u.Role == "User")// Filter users by role
            
            .ToListAsync();
            

            // Return the list of users
            var userDtos = UserDto.Mapping(users);
            return Ok(userDtos);
        }

        [HttpGet("getbyid{userId:guid}")]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var userDto = UserDto.Mapping(user);
            return Ok(userDto);
        }

        [HttpPost("createuser")]
        //[Authorize(Roles = "StoreAdmin ")]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
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

        [HttpPut("UpdateUser/{userId:guid}")]
        //[Authorize(Roles = "SuperAdmin,StoreAdmin")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UserDto userDto)
        {
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
            //existingUser.CreatedBy = userDto.CreatedBy;
            //existingUser.UpdatedBy = userDto.UpdatedBy;
            existingUser.Token = userDto.Token;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User updated successfully." });
        }

        [HttpDelete("deleteuser{userId:guid}")]
        //[Authorize(Roles = "SuperAdmin,StoreAdmin")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User deleted successfully." });
        }
    }
}
