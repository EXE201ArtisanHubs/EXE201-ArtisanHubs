using System;
using System.Collections.Generic;

namespace ArtisanHubs.DTOs.DTO.Reponse.Order
{
    public class ArtistOrderResponse
    {
        public int OrderId { get; set; }
        public long OrderCode { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime? CreatedAt { get; set; }
        
        // Customer Info
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        
        // Order Items
        public List<ArtistOrderItemResponse> OrderItems { get; set; } = new List<ArtistOrderItemResponse>();
        
        // Commission Info
        public decimal TotalCommission { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ArtistEarnings { get; set; }
    }

    public class ArtistOrderItemResponse
    {
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        
        // Commission for this item
        public decimal CommissionAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal PlatformShare { get; set; }
        public bool IsPaid { get; set; }
    }
}
