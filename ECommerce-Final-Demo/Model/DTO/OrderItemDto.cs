namespace ECommerce_Final_Demo.Model.DTO
{
    public class OrderItemDto
    {
        public Guid ItemId { get; set; }
        public string? ItemName { get; set; } // Add this property add it last
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public static OrderItemDto Mapping(OrderItem orderItem)
        {
            return new OrderItemDto
            {
                ItemId = orderItem.ItemId,
                Quantity = orderItem.Quantity,
                Price   = orderItem.Price
            };
        }
    }
}
