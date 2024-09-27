using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Models.ViewModels
{
    public class UserViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name can only contain alphabetic characters.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last name can only contain alphabetic characters.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email must be a valid format.")]
        [Remote(action: "IsEmailAvailable", controller: "Auth", ErrorMessage = "Email is already in use.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mobile number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(15, ErrorMessage = "Mobile number cannot be longer than 15 characters.")]
        public string MobileNumber { get; set; } = null!;

        [Required]
        public string Role { get; set; } = "User";

        public DateTime CreateDate { get; set; } = DateTime.Now;

        public DateTime? UpdateDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Active status is required.")]
        public bool IsActive { get; set; } = true;

        public string? Profile { get; set; }

        public string? StoreName { get; set; }

       
        public IFormFile? ProfileImage { get; set; }
        //[Required(ErrorMessage = "Store Name is required.")]
        public Guid? StoreId { get; set; }

        [Required(ErrorMessage = "Password is required.")]      
        public string Password { get; set; }
        public string? createdBy { get; set; }
        public string? updatedBy { get; set; }
        public static User FromViewModel(UserViewModel viewModel)
        {
            return new User
            {
                Id = viewModel.Id,
                FName = viewModel.FirstName,
                LName = viewModel.LastName,
                Email = viewModel.Email,
                Password = viewModel.Password,
                MobileNumber = viewModel.MobileNumber,
                Role = viewModel.Role,
                CreateDate = viewModel.CreateDate,
                UpdateDate = viewModel.UpdateDate,
                IsActive = viewModel.IsActive,
                Profile = viewModel.ProfileImage?.FileName,
                StoreId = viewModel.StoreId,
                CreatedBy = viewModel.createdBy,
                UpdatedBy = viewModel.updatedBy
            };
        }
        public static UserViewModel ToViewModel(User user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FName,
                LastName = user.LName,
                Password = user.Password,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                Role = user.Role,
                CreateDate = user.CreateDate,
                UpdateDate = user.UpdateDate,
                IsActive = user.IsActive,
                ProfileImage = null,
                Profile = user.Profile,
                StoreId = user.StoreId,
                StoreName = user.StoreName,
                createdBy=user.CreatedBy,
                updatedBy=user.UpdatedBy
            };
        }
    }
}
