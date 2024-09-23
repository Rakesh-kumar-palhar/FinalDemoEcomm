using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Models.ViewModels
{
   

    public class ItemViewModel
    {
        public Guid Id { get; set; }


        public string Name { get; set; } = null!;


        public ItemCategory Category { get; set; }

        // public int Category {  get; set; }


        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        public string? Image { get; set; }
        public IFormFile? ImageFile { get; set; }

        public string? StoreName { get; set; }
        public Guid StoreId { get; set; }

        
        public static Item FromViewModel(ItemViewModel viewModel)
        {
            
            return new Item
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                Category = (Models.ItemCategory)viewModel.Category,
                Price = viewModel.Price,
                Image = viewModel.ImageFile != null ? viewModel.ImageFile.FileName : null,
                StoreId = viewModel.StoreId
            };
        }
    }
}
