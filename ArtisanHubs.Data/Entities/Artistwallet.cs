using System;
using System.Collections.Generic;

namespace ArtisanHubs.Data.Entities;

public partial class Artistwallet
{
    public int WalletId { get; set; }

    public int ArtistId { get; set; }

    public decimal Balance { get; set; }

    public decimal PendingBalance { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Artistprofile Artist { get; set; } = null!;

    public virtual ICollection<Wallettransaction> Wallettransactions { get; set; } = new List<Wallettransaction>();
}
