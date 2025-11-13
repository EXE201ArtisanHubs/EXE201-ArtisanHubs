using System.ComponentModel.DataAnnotations;

namespace ArtisanHubs.DTOs.DTO.Request.Carts
{
    public class UpdateCartItemQuantityRequest
    {
        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
        public int Quantity { get; set; }
    }
}
