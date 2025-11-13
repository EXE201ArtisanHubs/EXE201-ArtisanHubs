using ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces;
using ArtisanHubs.Bussiness.Services.Carts.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtisanHubs.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IArtistProfileService _artistProfileService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        protected int GetCurrentAccountId()
        {
            var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(accountIdString))
            {
                // Lỗi này xảy ra nếu token hợp lệ nhưng lại thiếu claim ID,
                // cho thấy có vấn đề ở khâu tạo token.
                throw new InvalidOperationException("Account ID claim (NameIdentifier) not found in token.");
            }

            return int.Parse(accountIdString);
        }
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyCart()
        {
            // Gọi hàm được kế thừa, code gọn gàng hơn rất nhiều
            var accountId = GetCurrentAccountId();
            var result = await _cartService.GetCartByAccountIdAsync(accountId);
            // Return trực tiếp kết quả từ service
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            // Gọi hàm được kế thừa
            var accountId = GetCurrentAccountId();
            var result = await _cartService.AddToCartAsync(accountId, request);
            // Return trực tiếp kết quả từ service
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        /// <param name="cartItemId">ID của cart item cần xóa</param>
        /// <returns>Cart sau khi xóa item</returns>
        [HttpDelete("{cartItemId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var accountId = GetCurrentAccountId();
            var result = await _cartService.RemoveFromCartAsync(accountId, cartItemId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Remove product from cart by productId
        /// </summary>
        /// <param name="productId">ID của product cần xóa khỏi cart</param>
        /// <returns>Cart sau khi xóa product</returns>
        [HttpDelete("product/{productId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RemoveProductFromCart(int productId)
        {
            var accountId = GetCurrentAccountId();
            var result = await _cartService.RemoveProductFromCartAsync(accountId, productId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Update quantity of a cart item
        /// </summary>
        /// <param name="cartItemId">ID của cart item cần update</param>
        /// <param name="request">Request chứa quantity mới</param>
        /// <returns>Cart sau khi update quantity và tính lại total price</returns>
        [HttpPut("{cartItemId}/quantity")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateCartItemQuantity(int cartItemId, [FromBody] UpdateCartItemQuantityRequest request)
        {
            var accountId = GetCurrentAccountId();
            var result = await _cartService.UpdateCartItemQuantityAsync(accountId, cartItemId, request.Quantity);
            return StatusCode(result.StatusCode, result);
        }

    }
}
