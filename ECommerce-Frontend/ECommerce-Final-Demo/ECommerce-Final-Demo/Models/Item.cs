using ECommerce_Final_Demo.Models.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Models
{
    public enum ItemCategory
    {
        Veg,
        NonVeg,
        Egg
    }
    public class Item
    {
        public Guid Id { get; set; }


        public string Name { get; set; } = null!;


        public ItemCategory Category { get; set; }


        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }


        public string? Image { get; set; }

        public string? StoreName { get; set; }
        public Guid StoreId { get; set; }
        

        // Mapping from Item to ItemViewModel
        public static ItemViewModel ToViewModel(Item item)
        {
            return new ItemViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Category = (ViewModels.ItemCategory)item.Category,
                Price = item.Price,
                StoreId = item.StoreId,
                ImageFile = null ,
                StoreName = item.StoreName,
                Image = item.Image
            };
        }
    }
}
