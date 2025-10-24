using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.Products.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using ArtisanHubs.Data.Repositories.Products.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Reponse.Products;
using ArtisanHubs.DTOs.DTO.Request.Products;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Products.Implements
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly IMapper _mapper;
        private readonly PhotoService _photoService;
        private readonly IArtistProfileRepository _artistProfileRepo;
        public ProductService(IProductRepository productRepository, IMapper mapper, PhotoService photoService, IArtistProfileRepository artistProfileRepository)
        {
            _productRepo = productRepository;
            _mapper = mapper;
            _photoService = photoService;
            _artistProfileRepo = artistProfileRepository;
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

            // Update image if provided
            if (request.Images != null)
            {
                // Upload the image and get the URL
                var imageUrl = await _photoService.UploadImageAsync(request.Images);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    existingProduct.Images = imageUrl;
                }
            }

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

        //public async Task<ApiResponse<IEnumerable<ProductSummaryResponse>>> GetMyProductsAsync(int artistId)
        //{
        //    try
        //    {
        //        var products = await _productRepo.GetProductsByArtistIdAsync(artistId);
        //        // THAY ĐỔI Ở ĐÂY: Map sang DTO mới
        //        var response = _mapper.Map<IEnumerable<ProductSummaryResponse>>(products);
        //        return ApiResponse<IEnumerable<ProductSummaryResponse>>.SuccessResponse(response, "Get products successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse<IEnumerable<ProductSummaryResponse>>.FailResponse($"An error occurred: {ex.Message}", 500);
        //    }
        //}
        //public async Task<ApiResponse<ArtistShopResponse>> GetMyProductsAsync(int artistId)
        //{
        //    try
        //    {
        //        // 1. Lấy profile nghệ nhân bằng artistId
        //        var artistProfile = await _artistProfileRepo.GetByIdAsync(artistId);

        //        if (artistProfile == null)
        //        {
        //            return ApiResponse<ArtistShopResponse>.FailResponse("Artist not found.", 404);
        //        }

        //        // 2. Lấy danh sách sản phẩm của nghệ nhân
        //        var products = await _productRepo.GetProductsByArtistIdAsync(artistId);

        //        // 3. Kiểm tra nếu artist không có product nào
        //        if (products == null || !products.Any())
        //        {
        //            return ApiResponse<ArtistShopResponse>.FailResponse("This artist doesn't have any products yet.", 404);
        //        }

        //        // 4. Dùng AutoMapper để map các đối tượng entity sang DTO
        //        var profileResponse = _mapper.Map<ArtistProfileResponse>(artistProfile);
        //        var productsResponse = _mapper.Map<IEnumerable<ProductSummaryResponse>>(products);

        //        // 5. Tạo đối tượng response cuối cùng
        //        var shopResponse = new ArtistShopResponse
        //        {
        //            ArtistProfile = profileResponse,
        //            Products = productsResponse
        //        };

        //        return ApiResponse<ArtistShopResponse>.SuccessResponse(shopResponse, "Get artist shop successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse<ArtistShopResponse>.FailResponse($"An error occurred: {ex.Message}", 500);
        //    }
        //}

        public async Task<ApiResponse<ArtistShopResponse>> GetMyProductsAsync(int artistId)
        {
            try
            {
                // 1. Lấy profile nghệ nhân kèm theo danh sách sản phẩm
                var artistProfile = await _artistProfileRepo.GetProfileWithProductsAsync(artistId);

                if (artistProfile == null)
                {
                    return ApiResponse<ArtistShopResponse>.FailResponse("Artist not found.", 404);
                }

                // 2. Kiểm tra nếu artist không có product nào
                if (artistProfile.Products == null || !artistProfile.Products.Any())
                {
                    return ApiResponse<ArtistShopResponse>.FailResponse("This artist doesn't have any products yet.", 404);
                }

                // 2. Dùng AutoMapper để map các đối tượng entity sang DTO
                var profileResponse = _mapper.Map<ArtistProfileResponse>(artistProfile);
                var productsResponse = _mapper.Map<IEnumerable<ProductSummaryResponse>>(artistProfile.Products);

                // 3. Tạo đối tượng response cuối cùng
                var shopResponse = new ArtistShopResponse
                {
                    ArtistProfile = profileResponse,
                    Products = productsResponse
                };

                return ApiResponse<ArtistShopResponse>.SuccessResponse(shopResponse, "Get artist shop successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<ArtistShopResponse>.FailResponse($"An error occurred: {ex.Message}", 500);
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
                // Check for duplicate product name
                var productExists = await _productRepo.ProductExistsByNameAsync(artistId, request.Name);
                if (productExists)
                {
                    return ApiResponse<ProductResponse>.FailResponse(
                        $"A product with the name '{request.Name}' already exists in your shop.", 409);
                }

                var productEntity = _mapper.Map<Product>(request);
                productEntity.ArtistId = artistId;
                productEntity.CreatedAt = DateTime.UtcNow;

                // Handle image upload
                if (request.Images != null)
                {
                    var imageUrl = await _photoService.UploadImageAsync(request.Images);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        productEntity.Images = imageUrl;
                    }
                }

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

        //public async Task<ApiResponse<IPaginate<ProductSummaryResponse>>> GetAllProductAsync(int page, int size, string? searchTerm = null)
        //{
        //    try
        //    {
        //        // Lấy danh sách category có phân trang
        //        var result = await _productRepo.GetPagedAsync(null, page, size, searchTerm);
        //        var pagedSummary = _mapper.Map<IPaginate<ProductSummaryResponse>>(result);
        //        return ApiResponse<IPaginate<ProductSummaryResponse>>.SuccessResponse(
        //            pagedSummary,
        //            "Get paginated categories successfully"
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse<IPaginate<ProductSummaryResponse>>.FailResponse($"Error: {ex.Message}");
        //    }
        //}

        public async Task<ApiResponse<IPaginate<ProductDetailResponse>>> GetAllProductAsync(int page, int size, string? searchTerm = null)
        {
            try
            {
                // Lấy danh sách products có phân trang
                var result = await _productRepo.GetPagedWithDetailsAsync(null, page, size, searchTerm);

                // Map từ IPaginate<Product> sang IPaginate<ProductSummaryResponse>
                var mappedItems = _mapper.Map<IList<ProductDetailResponse>>(result.Items);

                // Tạo IPaginate<ProductSummaryResponse> mới với dữ liệu đã map
                var mappedResult = new Paginate<ProductDetailResponse>
                {
                    Items = mappedItems,
                    Page = result.Page,
                    Size = result.Size,
                    Total = result.Total,
                    TotalPages = result.TotalPages
                };

                return ApiResponse<IPaginate<ProductDetailResponse>>.SuccessResponse(
                    mappedResult,
                    "Get paginated products successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IPaginate<ProductDetailResponse>>.FailResponse($"Error: {ex.Message}");
            }
        }
        public async Task<ApiResponse<IPaginate<ProductSummaryResponse>>> SearchProductsByNameForCustomerAsync(string? name, int page, int size)
        {
            try
            {
                // Tạo predicate để lọc theo tên sản phẩm
                Expression<Func<Product, bool>>? predicate = null;

                if (!string.IsNullOrEmpty(name))
                {
                    predicate = p => p.Name.Contains(name);
                }

                // Gọi repository để lấy dữ liệu có phân trang
                var paginatedProducts = await _productRepo.GetPagedAsync(predicate, page, size);

                // Map từ IPaginate<Product> sang IPaginate<ProductSummaryResponse>
                var mappedItems = _mapper.Map<IList<ProductSummaryResponse>>(paginatedProducts.Items);

                // Tạo IPaginate<ProductSummaryResponse> mới với dữ liệu đã map
                var result = new Paginate<ProductSummaryResponse>
                {
                    Items = mappedItems,
                    Page = paginatedProducts.Page,
                    Size = paginatedProducts.Size,
                    Total = paginatedProducts.Total,
                    TotalPages = paginatedProducts.TotalPages
                };

                return ApiResponse<IPaginate<ProductSummaryResponse>>.SuccessResponse(
                    result,
                    "Search products successfully."
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IPaginate<ProductSummaryResponse>>.FailResponse($"An error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<IPaginate<ProductSummaryResponse>>> FilterProductsForCustomerAsync(ProductFilterRequest filterRequest)
        {
            try
            {
                // Build predicate for filtering
                Expression<Func<Product, bool>>? predicate = BuildFilterPredicate(filterRequest);

                // Build ordering function
                Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy = BuildOrderByFunction(filterRequest.SortBy, filterRequest.SortOrder);

                // Get filtered and paginated products
                var paginatedProducts = await _productRepo.GetFilteredProductsAsync(
                    predicate,
                    orderBy,
                    filterRequest.Page,
                    filterRequest.Size
                );

                // Map to ProductSummaryResponse
                var mappedItems = _mapper.Map<IList<ProductSummaryResponse>>(paginatedProducts.Items);

                // Create result with pagination info
                var result = new Paginate<ProductSummaryResponse>
                {
                    Items = mappedItems,
                    Page = paginatedProducts.Page,
                    Size = paginatedProducts.Size,
                    Total = paginatedProducts.Total,
                    TotalPages = paginatedProducts.TotalPages
                };

                return ApiResponse<IPaginate<ProductSummaryResponse>>.SuccessResponse(
                    result,
                    "Filter products successfully."
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IPaginate<ProductSummaryResponse>>.FailResponse($"An error occurred: {ex.Message}", 500);
            }
        }

        private Expression<Func<Product, bool>>? BuildFilterPredicate(ProductFilterRequest filterRequest)
        {
            Expression<Func<Product, bool>>? predicate = null;

            if (filterRequest.CategoryId.HasValue)
            {
                predicate = CombinePredicates(predicate, p => p.CategoryId == filterRequest.CategoryId.Value);
            }

            if (filterRequest.ArtistId.HasValue)
            {
                predicate = CombinePredicates(predicate, p => p.ArtistId == filterRequest.ArtistId.Value);
            }

            if (filterRequest.MinPrice.HasValue)
            {
                predicate = CombinePredicates(predicate, p => p.Price >= filterRequest.MinPrice.Value);
            }

            if (filterRequest.MaxPrice.HasValue)
            {
                predicate = CombinePredicates(predicate, p => p.Price <= filterRequest.MaxPrice.Value);
            }

            // SỬA Ở ĐÂY: Chỉ filter khi status được cung cấp, không có else nữa
            if (!string.IsNullOrEmpty(filterRequest.Status))
            {
                predicate = CombinePredicates(predicate, p => p.Status == filterRequest.Status);
            }
            // BỎ HOÀN TOÀN KHỐI ELSE

            if (!string.IsNullOrEmpty(filterRequest.Name))
            {
                predicate = CombinePredicates(predicate, p => p.Name.Contains(filterRequest.Name));
            }

            return predicate;
        }

        private Expression<Func<Product, bool>>? CombinePredicates(
            Expression<Func<Product, bool>>? expr1,
            Expression<Func<Product, bool>> expr2)
        {
            if (expr1 == null)
                return expr2;

            var parameter = Expression.Parameter(typeof(Product));
            var body = Expression.AndAlso(
                Expression.Invoke(expr1, parameter),
                Expression.Invoke(expr2, parameter)
            );
            return Expression.Lambda<Func<Product, bool>>(body, parameter);
        }

        private Func<IQueryable<Product>, IOrderedQueryable<Product>>? BuildOrderByFunction(string? sortBy, string? sortOrder)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query => query.OrderByDescending(p => p.CreatedAt);

            var isDescending = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToLower() == "desc";

            return sortBy.ToLower() switch
            {
                "price" => isDescending
                    ? query => query.OrderByDescending(p => p.Price)
                    : query => query.OrderBy(p => p.Price),
                "name" => isDescending
                    ? query => query.OrderByDescending(p => p.Name)
                    : query => query.OrderBy(p => p.Name),
                "createdat" => isDescending
                    ? query => query.OrderByDescending(p => p.CreatedAt)
                    : query => query.OrderBy(p => p.CreatedAt),
                _ => query => query.OrderByDescending(p => p.CreatedAt)
            };
        }

        //public async Task<ApiResponse<IPaginate<ProductSummaryResponse>>> GetAllProductsForCustomerAsync(int page = 1, int size = 10, string? searchTerm = null)
        //{
        //    try
        //    {
        //        // Tạo predicate để lọc sản phẩm có sẵn và search term nếu có
        //        Expression<Func<Product, bool>>? predicate = p => p.Status == "Available";

        //        if (!string.IsNullOrEmpty(searchTerm))
        //        {
        //            predicate = CombinePredicates(predicate, p => p.Name.Contains(searchTerm));
        //        }

        //        // Gọi repository để lấy dữ liệu có phân trang
        //        var paginatedProducts = await _productRepo.GetPagedAsync(predicate, page, size);

        //        // Map từ IPaginate<Product> sang IPaginate<ProductSummaryResponse>
        //        var mappedItems = _mapper.Map<IList<ProductSummaryResponse>>(paginatedProducts.Items);

        //        // Tạo IPaginate<ProductSummaryResponse> mới với dữ liệu đã map
        //        var result = new Paginate<ProductSummaryResponse>
        //        {
        //            Items = mappedItems,
        //            Page = paginatedProducts.Page,
        //            Size = paginatedProducts.Size,
        //            Total = paginatedProducts.Total,
        //            TotalPages = paginatedProducts.TotalPages
        //        };

        //        return ApiResponse<IPaginate<ProductSummaryResponse>>.SuccessResponse(
        //            result,
        //            "Get all products successfully."
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse<IPaginate<ProductSummaryResponse>>.FailResponse($"An error occurred: {ex.Message}", 500);
        //    }
        //}
    }
}