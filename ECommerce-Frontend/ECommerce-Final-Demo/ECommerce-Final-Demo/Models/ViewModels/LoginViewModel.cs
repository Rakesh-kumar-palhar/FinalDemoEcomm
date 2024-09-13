using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
           ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = null!;
        [Required]

        public string Password { get; set; } = null!;
    }
}
