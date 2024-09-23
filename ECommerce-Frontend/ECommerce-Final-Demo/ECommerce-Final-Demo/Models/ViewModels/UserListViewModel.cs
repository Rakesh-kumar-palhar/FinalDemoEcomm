namespace ECommerce_Final_Demo.Models.ViewModels
{
    public class UserListViewModel
    {
        public IEnumerable<UserViewModel> Users { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}
