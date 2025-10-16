using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ArtisanHubs.DTOs.DTO.Request.ArtistProfile
{
    public class ArtistProfileRequest
    {
        public string ArtistName { get; set; }
        public string? ShopName { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? Specialty { get; set; }
        public int? ExperienceYears { get; set; }
        public List<string> Achievements { get; set; } = new List<string>();
    }
}
