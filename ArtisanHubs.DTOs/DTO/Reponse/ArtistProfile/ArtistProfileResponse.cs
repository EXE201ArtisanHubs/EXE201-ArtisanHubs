using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile
{
    public class ArtistProfileResponse
    {
        public int ArtistId { get; set; }
        public int AccountId { get; set; }
        public string ArtistName { get; set; }
        public string? ShopName { get; set; }
        public string? ProfileImage { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? Specialty { get; set; }
        public int? ExperienceYears { get; set; }
        public List<AchievementResponse> Achievements { get; set; } = new List<AchievementResponse>();
    }
}
