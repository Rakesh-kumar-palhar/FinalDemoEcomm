using ECommerce_Final_Demo.Models.ViewModels;
using System.Security.Cryptography.X509Certificates;

namespace ECommerce_Final_Demo.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string FName { get; set; } = null!;
        public string LName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } // Note: Passwords are usually not included in DTOs for security reasons
        public string MobileNumber { get; set; } = null!;
        public string Role { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsActive { get; set; }
        public string? StoreName{ get; set; }
        public string? Profile { get; set; }
        public Guid? StoreId { get; set; }



       
        

    }
}