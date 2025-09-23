using System;
using System.Collections.Generic;

namespace ArtisanHubs.Data.Entities;

public partial class Category
{
    public int CategoryId { get; set; }

    public int? ParentId { get; set; }

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Category> InverseParent { get; set; } = new List<Category>();

    public virtual Category? Parent { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
