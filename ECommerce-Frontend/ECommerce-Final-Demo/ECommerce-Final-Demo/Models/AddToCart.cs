namespace ECommerce_Final_Demo.Models
{
    public class AddToCart
    {
        public Guid ItemId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
