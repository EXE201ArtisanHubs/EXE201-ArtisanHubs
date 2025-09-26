using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Reponse.Categories
{
    public class CategoryResponse
    {
        public int CategoryId { get; set; }
        public int? ParentId { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
    }
}
