using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Reponse.Products
{
   public class ProductForCustomerResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Story { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string? Images { get; set; }
        public string? CategoryName { get; set; }
        public string? ArtistName { get; set; }
        public double? AverageRating { get; set; }
        public int FavoriteCount { get; set; }
    }
}
