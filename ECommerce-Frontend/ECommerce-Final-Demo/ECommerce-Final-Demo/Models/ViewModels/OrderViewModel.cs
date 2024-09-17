namespace ECommerce_Final_Demo.Models.ViewModels
{
    public class OrderViewModel
    {
        public Guid OrderId { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }


        // Static method to map OrderViewModel to Order
        public static Order FromViewModel(OrderViewModel viewModel)
        {
            return new Order
            {
                
                OrderId = viewModel.OrderId,
                StoreId = viewModel.StoreId,
                StoreName = viewModel.StoreName,
                UserId = viewModel.UserId,
                UserName = viewModel.UserName,
                OrderDate = viewModel.OrderDate,
                TotalAmount = viewModel.TotalAmount
               
            };
        }
    }
}
