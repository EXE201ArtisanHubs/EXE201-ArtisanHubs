namespace ArtisanHubs.DTOs.DTO.Reponse.Admin
{
    public class RevenueStatisticsResponse
    {
        public List<string> Labels { get; set; } = new();
        public List<RevenueDataset> Datasets { get; set; } = new();
        public RevenueSummary Summary { get; set; } = new();
    }

    public class RevenueDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new();
        public string? BackgroundColor { get; set; }
        public string? BorderColor { get; set; }
    }

    public class RevenueSummary
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalPlatformCommission { get; set; }
        public decimal TotalArtistEarnings { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalOrders { get; set; }
        public string Period { get; set; } = string.Empty;
    }
}
