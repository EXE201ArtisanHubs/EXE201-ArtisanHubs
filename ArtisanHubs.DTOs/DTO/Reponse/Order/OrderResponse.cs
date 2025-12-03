using System;
using System.Collections.Generic;

namespace ArtisanHubs.DTOs.DTO.Reponse.Order
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public long OrderCode { get; set; }
        public int AccountId { get; set; }
        public string? AccountUsername { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? ShippingAddress { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string? PaymentMethod { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int TotalItems { get; set; }
    }
}
