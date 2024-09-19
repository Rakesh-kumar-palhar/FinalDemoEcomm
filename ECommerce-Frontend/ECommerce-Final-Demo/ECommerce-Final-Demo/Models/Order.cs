using ECommerce_Final_Demo.Models.ViewModels;

namespace ECommerce_Final_Demo.Models
{
    public class Order
    {
        public Guid StoreId { get; set; }


        public Guid? OrderId { get; set; }         
                                                   
        public string? StoreName { get; set; }     
        public Guid? UserId { get; set; }         
        public string? UserName { get; set; }      
        public DateTime OrderDate { get; set; }           
        public decimal? TotalAmount { get; set; }
        public bool Status { get; set; }

        public static OrderViewModel ToViewModel(Order order)
        {
            return new OrderViewModel
            {
                OrderId = (Guid)order.OrderId,
                StoreName = order.StoreName,
                UserId = (Guid)order.UserId,
                UserName = order.UserName,
                OrderDate = order.OrderDate,
                TotalAmount = (decimal)order.TotalAmount,
                Status= order.Status
            };
        }
    }

}
