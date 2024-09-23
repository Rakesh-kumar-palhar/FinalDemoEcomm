using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Models.ViewModels
{
    public class UserViewModel
    {
       
        public Guid Id { get; set; }
      
        public string FirstName { get; set; } = null!;
       
        public string LastName { get; set; } = null!;
       
        
        
        public string Email { get; set; } = null!;
       
        public string MobileNumber { get; set; } = null!;
        public string Role { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsActive { get; set; }
        
        public string? Profile { get; set; }
       
        public string? StoreName { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public Guid? StoreId { get; set; }
        
        public string Password { get; set; }


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
                StoreId = viewModel.StoreId
            };
        }

    }
}
