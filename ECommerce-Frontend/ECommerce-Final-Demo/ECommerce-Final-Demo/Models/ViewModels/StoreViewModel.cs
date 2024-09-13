namespace ECommerce_Final_Demo.Models.ViewModels
{
    public class StoreViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public int CountryId { get; set; }
        public int StateId { get; set; }
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
