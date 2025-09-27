using System;
using System.Collections.Generic;

namespace ArtisanHubs.Data.Entities;

public partial class Withdrawrequest
{
    public int WithdrawId { get; set; }

    public int ArtistId { get; set; }

    public decimal Amount { get; set; }

    public string BankName { get; set; } = null!;

    public string AccountHolder { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? RequestedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Artistprofile Artist { get; set; } = null!;

    public virtual ICollection<Wallettransaction> Wallettransactions { get; set; } = new List<Wallettransaction>();
}
