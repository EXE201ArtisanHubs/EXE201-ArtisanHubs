using System;
using System.Collections.Generic;

namespace ArtisanHubs.Data.Entities;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int AccountId { get; set; }

    public int? ProductId { get; set; }

    public int? WorkshopId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Product? Product { get; set; }

    public virtual Artistworkshop? Workshop { get; set; }
}
