using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.Products.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Products.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.Products;
using ArtisanHubs.DTOs.DTO.Request.Products;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Products.Implements
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepo = productRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ProductResponse?>> UpdateProductAsync(int productId, int artistId, UpdateProductRequest request)
        {
            var existingProduct = await _productRepo.GetByIdAsync(productId);

            // QUAN TRỌNG: Kiểm tra sản phẩm có tồn tại và có thuộc về đúng nghệ nhân không
            if (existingProduct == null || existingProduct.ArtistId != artistId)
            {
                return ApiResponse<ProductResponse?>.FailResponse("Product not found or you don't have permission.", 404);
            }

            _mapper.Map(request, existingProduct);
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _productRepo.UpdateAsync(existingProduct);
            var response = _mapper.Map<ProductResponse>(existingProduct);
            return ApiResponse<ProductResponse?>.SuccessResponse(response, "Product updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(int productId, int artistId)
        {
            var existingProduct = await _productRepo.GetByIdAsync(productId);

            // QUAN TRỌNG: Kiểm tra quyền sở hữu trước khi xóa
            if (existingProduct == null || existingProduct.ArtistId != artistId)
            {
                return ApiResponse<bool>.FailResponse("Product not found or you don't have permission.", 404);
            }

            await _productRepo.RemoveAsync(existingProduct);
            return ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully.");
        }

        public async Task<ApiResponse<IEnumerable<ProductSummaryResponse>>> GetMyProductsAsync(int artistId)
        {
            try
            {
                var products = await _productRepo.GetProductsByArtistIdAsync(artistId);
                // THAY ĐỔI Ở ĐÂY: Map sang DTO mới
                var response = _mapper.Map<IEnumerable<ProductSummaryResponse>>(products);
                return ApiResponse<IEnumerable<ProductSummaryResponse>>.SuccessResponse(response, "Get products successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<ProductSummaryResponse>>.FailResponse($"An error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<ProductResponse?>> GetMyProductByIdAsync(int productId, int artistId)
        {
            try
            {
                var product = await _productRepo.GetProductWithDetailsAsync(productId);

                if (product == null || product.ArtistId != artistId)
                {
                    return ApiResponse<ProductResponse?>.FailResponse("Product not found or you don't have permission.", 404);
                }

                var response = _mapper.Map<ProductResponse>(product);
                return ApiResponse<ProductResponse?>.SuccessResponse(response, "Get product successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductResponse?>.FailResponse($"An error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<ProductResponse>> CreateProductAsync(int artistId, CreateProductRequest request)
        {
            try
            {
                // --- THÊM LOGIC KIỂM TRA TÊN TRÙNG LẶP ---
                var productExists = await _productRepo.ProductExistsByNameAsync(artistId, request.Name);
                if (productExists)
                {
                    // Dùng mã lỗi 409 Conflict vì tài nguyên (tên sản phẩm) đã tồn tại
                    return ApiResponse<ProductResponse>.FailResponse($"A product with the name '{request.Name}' already exists in your shop.", 409);
                }

                var productEntity = _mapper.Map<Product>(request);
                productEntity.ArtistId = artistId;
                productEntity.CreatedAt = DateTime.UtcNow;

                await _productRepo.CreateAsync(productEntity);
                var response = _mapper.Map<ProductResponse>(productEntity);

                return ApiResponse<ProductResponse>.SuccessResponse(response, "Product created successfully.", 201);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductResponse>.FailResponse($"An unexpected error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<ProductDetailResponse>> GetProductByIdForCustomerAsync(int productId)
        {
            try
            {
                var product = await _productRepo.GetProductWithDetailsAsync(productId);
                if (product == null)
                {
                    return ApiResponse<ProductDetailResponse>.FailResponse("Product not found.", 404);
                }

                // THAY ĐỔI Ở ĐÂY: Map sang DTO mới
                var response = _mapper.Map<ProductDetailResponse>(product);
                return ApiResponse<ProductDetailResponse>.SuccessResponse(response, "Get product successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductDetailResponse>.FailResponse($"An error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<ProductSummaryResponse>>> GetProductsByCategoryIdForCustomerAsync(int categoryId)
        {
            try
            {
                var products = await _productRepo.GetProductsByCategoryIdAsync(categoryId);
                if (products == null || !products.Any())
                {
                    return ApiResponse<IEnumerable<ProductSummaryResponse>>.FailResponse("No products found in this category.", 404);
                }
                // THAY ĐỔI Ở ĐÂY: Map sang DTO mới
                var response = _mapper.Map<IEnumerable<ProductSummaryResponse>>(products);
                return ApiResponse<IEnumerable<ProductSummaryResponse>>.SuccessResponse(response, "Get products successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<ProductSummaryResponse>>.FailResponse($"An error occurred: {ex.Message}", 500);
            }
        }
    }
}