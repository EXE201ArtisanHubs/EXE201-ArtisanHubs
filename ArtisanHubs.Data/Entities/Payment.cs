using System;
using System.Collections.Generic;

namespace ArtisanHubs.Data.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? OrderId { get; set; }

    public int? WorkshopId { get; set; }

    public decimal Amount { get; set; }

    public string? Method { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual ICollection<Wallettransaction> Wallettransactions { get; set; } = new List<Wallettransaction>();

    public virtual Artistworkshop? Workshop { get; set; }
}
