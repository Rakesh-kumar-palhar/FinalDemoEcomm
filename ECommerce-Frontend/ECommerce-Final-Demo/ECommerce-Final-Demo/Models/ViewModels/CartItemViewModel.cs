﻿namespace ECommerce_Final_Demo.Models.ViewModels
{
    public class CartItemViewModel
    {
        public Guid ItemId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public string? ItemName { get; set; }
    }
}
