
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;

namespace ECommerce_Final_Demo.Model
{
    public class Store
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;
        [Required]
        [ForeignKey("country")]
        public int CountryId { get; set; } 
        [Required]
        [ForeignKey("State")]
        public int StateId { get; set; } 
        [Required]
        [ForeignKey("City")]
        public int CityId { get; set; } //Enum for city
        public string? Image { get; set; }
        public bool IsDelete { get; set; } = true;
        //relationship
        public virtual ICollection<User>? Users { get; set; }
        public virtual ICollection<Item>? Items { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
        public virtual Country Country { get; set; }
        public virtual City City { get; set; }
        public virtual State State { get; set; }

    }
    


}