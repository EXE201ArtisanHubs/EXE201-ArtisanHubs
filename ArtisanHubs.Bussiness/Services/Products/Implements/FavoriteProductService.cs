using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.Products.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Products.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.Products;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Products.Implements
{
    public class FavoriteProductService : IFavoriteProductService
    {
        private readonly IFavoriteProductRepository _favoriteRepo;
        private readonly IProductRepository _productRepo;
        private readonly IMapper _mapper;

        public FavoriteProductService(IFavoriteProductRepository favoriteRepo, IProductRepository productRepo, IMapper mapper)
        {
            _favoriteRepo = favoriteRepo;
            _productRepo = productRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<bool>> AddFavoriteAsync(int accountId, int productId)
        {
            try
            {
                if (await _productRepo.GetByIdAsync(productId) == null)
                {
                    return ApiResponse<bool>.FailResponse("Product not found.", 404);
                }

                if (await _favoriteRepo.GetFavoriteAsync(accountId, productId) != null)
                {
                    return ApiResponse<bool>.FailResponse("Product is already in your favorites.", 409);
                }

                var favorite = new FavoriteProduct { AccountId = accountId, ProductId = productId };
                await _favoriteRepo.CreateAsync(favorite);

                return ApiResponse<bool>.SuccessResponse(true, "Product added to favorites successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailResponse($"An error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> RemoveFavoriteAsync(int accountId, int productId)
        {
            try
            {
                var favorite = await _favoriteRepo.GetFavoriteAsync(accountId, productId);
                if (favorite == null)
                {
                    return ApiResponse<bool>.FailResponse("Product not found in your favorites.", 404);
                }

                await _favoriteRepo.RemoveAsync(favorite);
                return ApiResponse<bool>.SuccessResponse(true, "Product removed from favorites successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailResponse($"An error occurred: {ex.Message}", 500);
            }
        }

        // 💡 THIS METHOD HAS BEEN CORRECTED
        public async Task<ApiResponse<IEnumerable<ProductForCustomerResponse>>> GetMyFavoritesAsync(int accountId)
        {
            try
            {
                // 1. Bắt đầu từ Repository của Product, KHÔNG phải FavoriteProduct
                var favoriteProducts = await _productRepo.GetQueryable()
                    // 2. Include TẤT CẢ dữ liệu liên quan TRƯỚC TIÊN
                    .Include(p => p.Category)
                    .Include(p => p.Artist)
                    .Include(p => p.Feedbacks)
                    .Include(p => p.FavoriteProducts)
                    // 3. BÂY GIỜ mới lọc những Product có trong danh sách yêu thích của accountId
                    .Where(p => p.FavoriteProducts.Any(fp => fp.AccountId == accountId))
                    .ToListAsync();

                var response = _mapper.Map<IEnumerable<ProductForCustomerResponse>>(favoriteProducts);
                return ApiResponse<IEnumerable<ProductForCustomerResponse>>.SuccessResponse(response, "Get favorite products successfully.");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ở đây để debug dễ hơn
                // Ví dụ: _logger.LogError(ex, "Error in GetMyFavoritesAsync");
                return ApiResponse<IEnumerable<ProductForCustomerResponse>>.FailResponse($"An error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<ProductForCustomerResponse>>> GetTrendingProductsAsync(int topN = 10)
        {
            try
            {
                // PHẢI INCLUDE TẤT CẢ DỮ LIỆU MAPPER CẦN
                var trendingProducts = await _productRepo.GetQueryable()
                    .Include(p => p.FavoriteProducts)   // Cần cho OrderByDescending và FavoriteCount
                    .Include(p => p.Category)           // Cần cho CategoryName
                    .Include(p => p.Artist)             // Cần cho ArtistName
                    .Include(p => p.Feedbacks)          // <-- THÊM DÒNG NÀY: Cần cho AverageRating
                    .OrderByDescending(p => p.FavoriteProducts.Count)
                    .Take(topN)
                    .ToListAsync();

                var response = _mapper.Map<IEnumerable<ProductForCustomerResponse>>(trendingProducts);
                return ApiResponse<IEnumerable<ProductForCustomerResponse>>.SuccessResponse(response, "Get trending products successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<ProductForCustomerResponse>>.FailResponse($"An error occurred: {ex.Message}", 500);
            }
        }
    }
}
