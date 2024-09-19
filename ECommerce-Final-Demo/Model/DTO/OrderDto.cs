namespace ECommerce_Final_Demo.Model.DTO
{
    public class OrderDto
    {
        public Guid StoreId { get; set; }
        public Guid? OrderId { get; set; }         // Unique ID of the Order
              
        public string? StoreName { get; set; }     // Name of the Store
        public Guid? UserId { get; set; }          // ID of the User who placed the order
        public string? UserName { get; set; }      // Name of the User
        public DateTime OrderDate { get; set; }   // Date when the Order was placed
        public bool Status { get; set; }
        // Optionally, you can add more fields as needed, such as:
        public decimal? TotalAmount { get; set; }  // Total amount of the Order
       
    }
}
