using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce_Final_Demo.Model
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; } 

        [Required]
        public Guid UserId { get; set; } 

        [Required]
        public Guid StoreId { get; set; } 

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; 

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "TotalAmount must be greater than zero.")]
        public decimal TotalAmount { get; set; } 

        //Relationship
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("StoreId")]
        public Store?Store { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
