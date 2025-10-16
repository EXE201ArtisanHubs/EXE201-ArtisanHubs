using ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Reponse.Products
{
    public class ArtistShopResponse
    {
        public ArtistProfileResponse ArtistProfile { get; set; }
        public IEnumerable<ProductSummaryResponse> Products { get; set; }
    }
}
