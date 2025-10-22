using ArtisanHubs.Bussiness.Services.Products.Interfaces;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtisanHubs.API.Controllers
{
    
    [Route("api/my-products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IArtistProfileRepository _artistProfileRepo;

        public ProductController(IProductService productService, IArtistProfileRepository artistProfileRepo)
        {
            _productService = productService;
            _artistProfileRepo = artistProfileRepo;
        }

        // ✅ HÀM HELPER MỚI: Chỉ lấy AccountId từ token
        private int GetCurrentAccountId()
        {
            var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountIdString))
            {
                throw new InvalidOperationException("Claim 'NameIdentifier' (Account ID) not found in token.");
            }
            if (!int.TryParse(accountIdString, out var accountId))
            {
                throw new InvalidOperationException("Claim 'NameIdentifier' in token is not a valid integer.");
            }
            return accountId;
        }

        //[Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategories(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? searchTerm = null)
        {
            var result = await _productService.GetAllProductAsync(page, size, searchTerm);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Artist")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request)
        {
            // Bước 1: Lấy AccountId từ token
            var accountId = GetCurrentAccountId();

            // Bước 2: Dùng AccountId để lấy thông tin ArtistProfile
            var artistProfile = await _artistProfileRepo.GetProfileByAccountIdAsync(accountId);
            if (artistProfile == null)
            {
                return Unauthorized("Artist profile not found for this account.");
            }

            // Bước 3: Gọi service với ArtistId
            var result = await _productService.CreateProductAsync(artistProfile.ArtistId, request);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Artist")]
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetMyProductById(int productId)
        {
            var accountId = GetCurrentAccountId();
            var artistProfile = await _artistProfileRepo.GetProfileByAccountIdAsync(accountId);
            if (artistProfile == null) return Unauthorized("Artist profile not found for this account.");

            var result = await _productService.GetMyProductByIdAsync(productId, artistProfile.ArtistId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpGet("customer/{productId}")]
        public async Task<IActionResult> GetProductByIdForCustomer(int productId)
        {
            var result = await _productService.GetProductByIdForCustomerAsync(productId);
            return StatusCode(result.StatusCode, result);
        }

        //[Authorize]
        [HttpGet("artist/{artistId}/products")]
        public async Task<IActionResult> GetProductsByArtist(int artistId)
        {
            var result = await _productService.GetMyProductsAsync(artistId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Artist")]
        [HttpGet]
        public async Task<IActionResult> GetMyProducts()
        {
            var accountId = GetCurrentAccountId();
            var artistProfile = await _artistProfileRepo.GetProfileByAccountIdAsync(accountId);
            if (artistProfile == null)
            {
                return Unauthorized("Artist profile not found for this account.");
            }

            var result = await _productService.GetMyProductsAsync(artistProfile.ArtistId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Artist")]
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromForm] UpdateProductRequest request)
        {
            var accountId = GetCurrentAccountId();
            var artistProfile = await _artistProfileRepo.GetProfileByAccountIdAsync(accountId);
            if (artistProfile == null) return Unauthorized("Artist profile not found for this account.");

            var result = await _productService.UpdateProductAsync(productId, artistProfile.ArtistId, request);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Artist")]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var accountId = GetCurrentAccountId();
            var artistProfile = await _artistProfileRepo.GetProfileByAccountIdAsync(accountId);
            if (artistProfile == null) return Unauthorized("Artist profile not found for this account.");

            var result = await _productService.DeleteProductAsync(productId, artistProfile.ArtistId);
            return StatusCode(result.StatusCode, result);
        }

        //[Authorize]
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategoryIdForCustomer(int categoryId)
        {
            var result = await _productService.GetProductsByCategoryIdForCustomerAsync(categoryId);
            return StatusCode(result.StatusCode, result);
        }

        //[Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchProductsByName(
            [FromQuery] string? name = null,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            var result = await _productService.SearchProductsByNameForCustomerAsync(name, page, size);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Filter products for customers with multiple criteria
        /// </summary>
        //[Authorize]
        [HttpGet("filter")]
        public async Task<IActionResult> FilterProducts([FromQuery] ProductFilterRequest filterRequest)
        {
            var result = await _productService.FilterProductsForCustomerAsync(filterRequest);
            return StatusCode(result.StatusCode, result);
        }

        ///// <summary>
        ///// Get all available products for customers with pagination and search
        ///// </summary>
        //[HttpGet("all-products")]
        //public async Task<IActionResult> GetAllProductsForCustomer(
        //    [FromQuery] int page = 1,
        //    [FromQuery] int size = 10,
        //    [FromQuery] string? searchTerm = null)
        //{
        //    var result = await _productService.GetAllProductsForCustomerAsync(page, size, searchTerm);
        //    return StatusCode(result.StatusCode, result);
        //}
    }
}