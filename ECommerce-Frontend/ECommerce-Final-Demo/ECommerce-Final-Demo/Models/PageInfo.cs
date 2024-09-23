namespace ECommerce_Final_Demo.Models
{
    public class PageInfo
    {
        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }
        public int ItemPerPage { get; set; }
        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemPerPage);

    }
}
