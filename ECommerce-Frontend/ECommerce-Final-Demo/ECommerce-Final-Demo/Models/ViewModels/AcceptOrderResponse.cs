namespace ECommerce_Final_Demo.Models.ViewModels
{
    public class AcceptOrderResponse
    {
        public string Message { get; set; }
        public BillDetails Bill { get; set; }
    }
    public class BillDetails
    {
        public Guid OrderId { get; set; }
        public string UserName { get; set; }
        public string StoreName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
