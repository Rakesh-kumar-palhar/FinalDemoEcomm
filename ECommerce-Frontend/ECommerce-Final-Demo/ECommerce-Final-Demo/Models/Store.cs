using ECommerce_Final_Demo.Models.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Models
{
    public class Store
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;
        [Required]
        public int CountryId { get; set; }
        [Required]
        public int StateId { get; set; }
        [Required]
        public int CityId { get; set; }
        public string? Image { get; set; }

        // Relationships
        public virtual Country Country { get; set; }
        public virtual State State { get; set; }
        public virtual City City { get; set; }

        public static StoreViewModel ToViewModel(Store store)
        {
            return new StoreViewModel
            {
                Id = store.Id,
                Name = store.Name,
                CountryId = store.CountryId,
                StateId = store.StateId,
                CityId = store.CityId,               
                ImageFile = null,
                Image = store.Image,
                
            };
        }
    }
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class State
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    
}
