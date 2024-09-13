using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Model
{
    public class State
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Country")]
        [Required]
        public int CountryId { get; set; }

        [Required]
        public string Name { get; set; }

        // Navigation Properties
        public virtual Country Country { get; set; }
        public ICollection<City> Citys { get; set; }
        

    }
}
