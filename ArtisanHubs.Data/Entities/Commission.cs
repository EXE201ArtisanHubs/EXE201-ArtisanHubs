using System;
using System.Collections.Generic;

namespace ArtisanHubs.Data.Entities;

public partial class Commission
{
    public int CommissionId { get; set; }

    public int? ProductId { get; set; }

    public int? WorkshopId { get; set; }

    public int ArtistId { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public decimal Rate { get; set; }

    public decimal AdminShare { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Artistprofile Artist { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual Product? Product { get; set; }

    public virtual ICollection<Wallettransaction> Wallettransactions { get; set; } = new List<Wallettransaction>();

    public virtual Artistworkshop? Workshop { get; set; }
}
