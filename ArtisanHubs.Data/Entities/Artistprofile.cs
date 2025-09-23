using System;
using System.Collections.Generic;

namespace ArtisanHubs.Data.Entities;

public partial class Artistprofile
{
    public int ArtistId { get; set; }

    public int AccountId { get; set; }

    public string ArtistName { get; set; } = null!;

    public string? ShopName { get; set; }

    public string? ProfileImage { get; set; }

    public string? Bio { get; set; }

    public string? Location { get; set; }

    public string? SocialLinks { get; set; }

    public string? VerifiedStatus { get; set; }

    public decimal? AvgRating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Artistwallet? Artistwallet { get; set; }

    public virtual ICollection<Artistworkshop> Artistworkshops { get; set; } = new List<Artistworkshop>();

    public virtual ICollection<Commission> Commissions { get; set; } = new List<Commission>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Withdrawrequest> Withdrawrequests { get; set; } = new List<Withdrawrequest>();
}
