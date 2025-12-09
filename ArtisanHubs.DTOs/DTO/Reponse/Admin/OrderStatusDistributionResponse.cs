namespace ArtisanHubs.DTOs.DTO.Reponse.Admin
{
    public class OrderStatusDistributionResponse
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Data { get; set; } = new();
        public List<string> Colors { get; set; } = new();
        public OrderStatusSummary Summary { get; set; } = new();
    }

    public class OrderStatusSummary
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PaidOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal DeliveryRate { get; set; }
        public decimal CancellationRate { get; set; }
    }
}
