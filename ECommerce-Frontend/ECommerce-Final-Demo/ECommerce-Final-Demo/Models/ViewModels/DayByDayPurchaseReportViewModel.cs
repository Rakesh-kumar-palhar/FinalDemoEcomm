using System;
using System.Collections.Generic;



namespace ECommerce_Final_Demo.Models
{

    

    public class DayByDayPurchaseReportViewModel
    {
        public DateTime Date { get; set; }
        public string StoreName { get; set; }
        public decimal TotalSales { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
    }

    public class OrderItemViewModel
    {
        public Guid ItemId { get; set; }
        public string itemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;
    }

}