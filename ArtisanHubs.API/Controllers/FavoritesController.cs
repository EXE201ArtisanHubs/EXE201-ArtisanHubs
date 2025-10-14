using ArtisanHubs.Bussiness.Services.Products.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtisanHubs.API.Controllers
{
    [Route("api/favorites")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteProductService _favoriteService;

        public FavoritesController(IFavoriteProductService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        private int GetCurrentAccountId()
        {
            var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountIdString) || !int.TryParse(accountIdString, out var accountId))
            {
                throw new InvalidOperationException("Claim 'NameIdentifier' (Account ID) is invalid or not found in token.");
            }
            return accountId;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteProductRequest request)
        {
            var accountId = GetCurrentAccountId();
            var result = await _favoriteService.AddFavoriteAsync(accountId, request.ProductId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Customer")]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFavorite(int productId)
        {
            var accountId = GetCurrentAccountId();
            var result = await _favoriteService.RemoveFavoriteAsync(accountId, productId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("my-favorites")]
        public async Task<IActionResult> GetMyFavorites()
        {
            var accountId = GetCurrentAccountId();
            var result = await _favoriteService.GetMyFavoritesAsync(accountId);
            return StatusCode(result.StatusCode, result);
        }

        [AllowAnonymous] // Mọi người đều có thể xem sản phẩm trending
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingProducts([FromQuery] int top = 10)
        {
            var result = await _favoriteService.GetTrendingProductsAsync(top);
            return StatusCode(result.StatusCode, result);
        }
    }
}
