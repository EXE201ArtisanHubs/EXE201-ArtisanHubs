using System;

namespace ArtisanHubs.DTOs.DTO.Reponse.Transaction
{
    public class TransactionResponse
    {
        public int TransactionId { get; set; }
        public int WalletId { get; set; }
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public int? CommissionId { get; set; }
        public int? WithdrawId { get; set; }
        public int? PaymentId { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        
        // Related information
        public string OrderCode { get; set; }
        public string BankName { get; set; }
    }
}
