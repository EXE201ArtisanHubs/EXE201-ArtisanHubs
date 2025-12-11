using System;
using System.Collections.Generic;

namespace ArtisanHubs.DTOs.DTO.Reponse.Order
{
    public class CustomerOrderResponse
    {
        public int OrderId { get; set; }
        public long OrderCode { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Order Items
        public List<CustomerOrderItemResponse> OrderItems { get; set; } = new List<CustomerOrderItemResponse>();
        
        // Summary
        public int TotalItems { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class CustomerOrderItemResponse
    {
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        
        // Artist Info
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }
    }
}
