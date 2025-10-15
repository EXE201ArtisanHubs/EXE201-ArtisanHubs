using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ArtisanHubs.DTOs.DTO.Request.Products
{
    public class CreateProductRequest
    {
        public int? CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Story { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public IFormFile? Images { get; set; }
        public string Status { get; set; } = "Available"; // Mặc định khi tạo
    }
}
