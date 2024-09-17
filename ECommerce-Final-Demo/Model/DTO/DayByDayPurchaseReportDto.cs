namespace ECommerce_Final_Demo.Model.DTO
{
    public class DayByDayPurchaseReportDto
    {
        public DateTime Date { get; set; }
        public string StoreName { get; set; }
        public decimal TotalSales { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
