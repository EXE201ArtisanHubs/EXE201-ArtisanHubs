using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Reponse.Carts
{
        public class CartItemResponse
        {
            public int CartItemId { get; set; }

            public int ProductId { get; set; }
            public string ProductName { get; set; } = null!;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string? ImageUrl { get; set; }
        }
}
