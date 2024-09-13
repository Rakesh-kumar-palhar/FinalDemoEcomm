using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Model
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string LName { get; set; }= null!;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [Required]
        [StringLength(500)]
        public string? Password { get; set; }

        [Required]
        [StringLength(20)]
        public string? MobileNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "User"; 

        [Required]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateDate { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public string? Profile { get; set; } 

        [ForeignKey("Store")]
        public Guid? StoreId { get; set; }

              
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; } 

        [StringLength(500)]
        public string? Token { get; set; } 
        //relationship
        public Store? Store { get; set; }

        public Cart? Cart { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
