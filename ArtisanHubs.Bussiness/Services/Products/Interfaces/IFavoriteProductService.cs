using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.DTOs.DTO.Reponse.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Products.Interfaces
{
    public interface IFavoriteProductService
    {
        Task<ApiResponse<bool>> AddFavoriteAsync(int accountId, int productId);
        Task<ApiResponse<bool>> RemoveFavoriteAsync(int accountId, int productId);
        Task<ApiResponse<IEnumerable<ProductDetailResponse>>> GetMyFavoritesAsync(int accountId);
        Task<ApiResponse<IEnumerable<ProductDetailResponse>>> GetTrendingProductsAsync(int topN = 10);
    }
}
