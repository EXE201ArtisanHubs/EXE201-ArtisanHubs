using System;
using System.Collections.Generic;

namespace ArtisanHubs.Data.Entities;

public partial class Wallettransaction
{
    public int TransactionId { get; set; }

    public int WalletId { get; set; }

    public decimal Amount { get; set; }

    public string TransactionType { get; set; } = null!;

    public int? CommissionId { get; set; }

    public int? WithdrawId { get; set; }

    public int? PaymentId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Commission? Commission { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual Artistwallet Wallet { get; set; } = null!;

    public virtual Withdrawrequest? Withdraw { get; set; }
}
