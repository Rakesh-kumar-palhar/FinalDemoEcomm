using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce_Final_Demo.Model
{
    public class Item
    {
        public enum Category
        {
            Veg,
            NonVeg,
            Egg
        }

            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            [StringLength(200)]
            public string? Name { get; set; } 

            [Required]
            public Category Type { get; set; } 

            [Required]
            [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
            public decimal Price { get; set; }

            [Required]
            public string? Image { get; set; }
            

            //Relationship
            [Required]
            public Guid? StoreId { get; set; } 
            [ForeignKey("StoreId")]
            public Store? Store { get; set; }

            public ICollection<Cart>? Carts { get; set; }
            public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
