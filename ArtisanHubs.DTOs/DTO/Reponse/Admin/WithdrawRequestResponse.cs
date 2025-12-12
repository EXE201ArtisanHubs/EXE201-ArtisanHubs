namespace ArtisanHubs.DTOs.DTO.Reponse.Admin;

public class WithdrawRequestResponse
{
    public int WithdrawId { get; set; }
    public int ArtistId { get; set; }
    public string ArtistName { get; set; } = null!;
    public string ArtistEmail { get; set; } = null!;
    public string? ArtistPhone { get; set; }
    public decimal Amount { get; set; }
    public string BankName { get; set; } = null!;
    public string AccountHolder { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime? RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    
    // Wallet information
    public decimal CurrentWalletBalance { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal TotalBalance { get; set; }
    
    // History
    public int TotalWithdrawRequests { get; set; }
    public decimal TotalWithdrawnAmount { get; set; }
}
