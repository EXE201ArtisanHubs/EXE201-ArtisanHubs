using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.DTOs.DTO.Reponse.Carts;
using ArtisanHubs.DTOs.DTO.Request.Carts;

namespace ArtisanHubs.Bussiness.Services.Carts.Interfaces
{
    public interface ICartService
    {
        Task<Cart?> GetCartByIdAsync(int cartId);
        Task<ApiResponse<CartResponse>> AddToCartAsync(int accountId, AddToCartRequest request);
        Task<ApiResponse<CartResponse>> GetCartByAccountIdAsync(int accountId);
        Task<ApiResponse<CartResponse>> RemoveFromCartAsync(int accountId, int cartItemId);
        Task<ApiResponse<CartResponse>> RemoveProductFromCartAsync(int accountId, int productId);
        Task<ApiResponse<CartResponse>> UpdateCartItemQuantityAsync(int accountId, int cartItemId, int quantity);
    }
}
