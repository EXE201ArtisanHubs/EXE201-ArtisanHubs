namespace ArtisanHubs.DTOs.DTO.Reponse.Admin
{
    public class AdminOrderDetailResponse
    {
        public int OrderId { get; set; }
        public long OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        
        // Customer info
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        
        // Order items with commission breakdown
        public List<AdminOrderItemResponse> OrderItems { get; set; } = new();
        
        // Summary
        public int TotalItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalPlatformCommission { get; set; }
        public decimal TotalArtistEarnings { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AdminOrderItemResponse
    {
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        
        // Artist info
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        
        // Commission breakdown
        public int? CommissionId { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal ArtistEarning { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
