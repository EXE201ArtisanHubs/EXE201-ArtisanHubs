namespace ArtisanHubs.DTOs.DTO.Reponse.Admin
{
    public class OrderStatisticsResponse
    {
        // Overall statistics
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PaidOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        
        // Revenue statistics
        public decimal TotalRevenue { get; set; }
        public decimal TotalPlatformCommission { get; set; }
        public decimal TotalArtistEarnings { get; set; }
        public decimal TotalShippingFees { get; set; }
        
        // Commission details
        public decimal PaidCommissions { get; set; }
        public decimal UnpaidCommissions { get; set; }
        public int TotalCommissionRecords { get; set; }
        
        // Period info
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
