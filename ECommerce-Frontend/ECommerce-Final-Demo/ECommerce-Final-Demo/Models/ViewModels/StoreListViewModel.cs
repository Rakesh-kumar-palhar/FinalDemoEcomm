namespace ECommerce_Final_Demo.Models.ViewModels
{
    public class StoreListViewModel
    {
        public IEnumerable<StoreViewModel>Stores { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}
