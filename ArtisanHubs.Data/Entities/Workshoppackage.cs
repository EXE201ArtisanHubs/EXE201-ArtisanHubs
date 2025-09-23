using System;
using System.Collections.Generic;

namespace ArtisanHubs.Data.Entities;

public partial class Workshoppackage
{
    public int PackageId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? Duration { get; set; }

    public int? MaxViewers { get; set; }

    public decimal? CommissionRate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Artistworkshop> Artistworkshops { get; set; } = new List<Artistworkshop>();
}
