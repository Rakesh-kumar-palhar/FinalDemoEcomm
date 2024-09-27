using System.ComponentModel.DataAnnotations;

namespace ECommerce_Final_Demo.Models.ViewModels
{
    public class StoreViewModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Store name is required.")]
        [StringLength(50, ErrorMessage = "Store name cannot be longer than 50 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Store name can only contain alphabetic characters.")]
        public string Name { get; set; } = null!;
        [Required(ErrorMessage = "Country name is required.")]
        public int CountryId { get; set; }
        [Required(ErrorMessage = "State name is required.")]
        public int StateId { get; set; }
        [Required(ErrorMessage = "City name is required.")]
        public int CityId { get; set; }
       
        public string? Country { get; set; }
       
        public string? State { get; set; }
        
        public string? City { get; set; } 
        public string? Image { get; set; }
        public IFormFile? ImageFile { get; set; }

        public static Store FromViewModel(StoreViewModel viewModel)
        {
            return new Store
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                CountryId = viewModel.CountryId,
                StateId = viewModel.StateId,
                CityId = viewModel.CityId,

                Image = viewModel.ImageFile?.FileName,
               
            };
        }
    }
}
