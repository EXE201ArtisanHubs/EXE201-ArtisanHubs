namespace ArtisanHubs.DTOs.DTO.Reponse.Admin
{
    public class ArtistManagementResponse
    {
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        
        // Account info
        public int AccountId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        
        // Statistics
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int SoldProducts { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommissionEarned { get; set; }
        public decimal WalletBalance { get; set; }
        public decimal PendingBalance { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
