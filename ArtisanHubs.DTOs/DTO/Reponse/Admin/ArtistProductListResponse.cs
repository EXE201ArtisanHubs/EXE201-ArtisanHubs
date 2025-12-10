namespace ArtisanHubs.DTOs.DTO.Reponse.Admin
{
    public class ArtistProductListResponse
    {
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        
        public List<ArtistProductItemResponse> Products { get; set; } = new();
        
        // Summary
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }

    public class ArtistProductItemResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Images { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        
        // Category
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        
        // Sales info
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
