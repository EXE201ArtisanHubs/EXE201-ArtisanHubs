using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.DTOs.DTO.Reponse.Products;
using ArtisanHubs.DTOs.DTO.Request.Products;

namespace ArtisanHubs.Bussiness.Services.Products.Interfaces
{
    public interface IProductService
    {
        //Task<ApiResponse<IEnumerable<ProductSummaryResponse>>> GetMyProductsAsync(int artistId);
        Task<ApiResponse<ArtistShopResponse>> GetMyProductsAsync(int artistId);
        Task<ApiResponse<ProductResponse?>> GetMyProductByIdAsync(int productId, int artistId);        
        Task<ApiResponse<ProductResponse>> CreateProductAsync(int artistId, CreateProductRequest request);       
        Task<ApiResponse<ProductResponse?>> UpdateProductAsync(int productId, int artistId, UpdateProductRequest request);
        Task<ApiResponse<bool>> DeleteProductAsync(int productId, int artistId);
        Task<ApiResponse<ProductDetailResponse>> GetProductByIdForCustomerAsync(int productId);
        Task<ApiResponse<IEnumerable<ProductSummaryResponse>>> GetProductsByCategoryIdForCustomerAsync(int categoryId);
        Task<ApiResponse<IPaginate<Product>>> GetAllProductAsync(int page, int size, string? searchTerm = null);
        Task<ApiResponse<IPaginate<ProductSummaryResponse>>> SearchProductsByNameForCustomerAsync(string? name, int page, int size);
    }
}
