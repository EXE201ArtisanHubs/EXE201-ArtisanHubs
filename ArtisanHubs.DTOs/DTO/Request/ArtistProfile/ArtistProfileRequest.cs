using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Request.ArtistProfile
{
    public class ArtistProfileRequest
    {
        public string ArtistName { get; set; }
        public string? ShopName { get; set; }
        public string? ProfileImage { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
    }
}
