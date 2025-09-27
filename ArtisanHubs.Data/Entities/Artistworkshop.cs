using System;
using System.Collections.Generic;

namespace ArtisanHubs.Data.Entities;

public partial class Artistworkshop
{
    public int WorkshopId { get; set; }

    public int ArtistId { get; set; }

    public int PackageId { get; set; }

    public string Topic { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = null!;

    public int? ViewerCount { get; set; }

    public decimal? Revenue { get; set; }

    public virtual Artistprofile Artist { get; set; } = null!;

    public virtual ICollection<Commission> Commissions { get; set; } = new List<Commission>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual Workshoppackage Package { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
