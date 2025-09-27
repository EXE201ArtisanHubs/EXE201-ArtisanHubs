using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Request.WorkshopPackages
{
    public class WorkshopPackageRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int? Duration { get; set; }
        public int? MaxViewers { get; set; }
        public decimal? CommissionRate { get; set; }
        public string Status { get; set; } = null!;
    }
}
