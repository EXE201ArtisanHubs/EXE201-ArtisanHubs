using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces;
using ArtisanHubs.Bussiness.Services.Products.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.ArtistProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtisanHubs.API.Controllers
{
    [Route("api/artist-profiles")]
    [ApiController]
    public class ArtistProfilesController : ControllerBase
    {
        private readonly IArtistProfileService _artistProfileService;
        private readonly IProductService _productService;

        public ArtistProfilesController(IArtistProfileService artistProfileService,IProductService productService)
        {
            _artistProfileService = artistProfileService;
            _productService = productService;
        }
       
        private int GetCurrentAccountId()
        {
            // Xóa hoặc comment out toàn bộ khối #if DEBUG ... #endif
            // Chỉ giữ lại phần code đọc ID từ token

            var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(accountIdString))
            {
                // Lỗi này xảy ra nếu token hợp lệ nhưng lại thiếu claim ID của người dùng,
                // cho thấy có vấn đề ở khâu tạo token.
                throw new InvalidOperationException("Account ID claim (NameIdentifier) not found in token.");
            }

            return int.Parse(accountIdString);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var accountId = GetCurrentAccountId();

            // ✅ SỬA: Sử dụng ProductService thay vì ArtistProfileService
            var result = await _productService.GetMyProfileWithProductsAsync(accountId);

            return StatusCode(result.StatusCode, result);
        }

        // POST: api/artist-profiles/me
        // Tạo profile cho chính nghệ nhân đang đăng nhập
        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> CreateMyProfile([FromForm] ArtistProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var accountId = GetCurrentAccountId();
            var result = await _artistProfileService.CreateMyProfileAsync(accountId, request);

            return StatusCode(result.StatusCode, result);
        }

        // PUT: api/artist-profiles/me
        // Cập nhật profile cho chính nghệ nhân đang đăng nhập
        [Authorize(Roles = "Artist")]
        [HttpPut()]
        public async Task<IActionResult> UpdateMyProfile([FromForm] ArtistProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var accountId = GetCurrentAccountId();
            var result = await _artistProfileService.UpdateMyProfileAsync(accountId, request);

            return StatusCode(result.StatusCode, result);
        }

        // === CÁC ENDPOINT CÔNG KHAI HOẶC DÀNH CHO ADMIN ===


        // Lấy danh sách tất cả profile (có thể cho Admin hoặc công khai)
        //[Authorize(Roles = "Admin, Customer")]
        [HttpGet()]
        public async Task<IActionResult> GetAllPaged(
    [FromQuery] int page = 1,
    [FromQuery] int size = 10,
    [FromQuery] string? searchTerm = null
)
        {
            var result = await _artistProfileService.GetAllProfilesAsync(page, size, searchTerm);
            return Ok(result);
        }

        // Xóa một profile (chức năng này thường dành cho Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Artist")]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            var result = await _artistProfileService.DeleteProfileAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        ////[Authorize(Roles = "Customer,Artist")]
        //[HttpGet("artists")]
        //public async Task<IActionResult> GetAllArtists()
        //{
        //    var result = await _artistProfileService.GetAllArtistsAsync();
        //    return StatusCode(result.StatusCode, result);
        //}
        [HttpPost("withdraw-request")]
        public async Task<IActionResult> CreateWithdrawRequest([FromBody] CreateWithdrawRequestModel model)
        {
            var accountId = GetCurrentAccountId();
            var artistId = await _artistProfileService.GetArtistIdByAccountIdAsync(accountId);
            if (artistId == null)
                return NotFound("Artist profile not found for this account.");
            var result = await _artistProfileService.CreateWithdrawRequestAsync(
                (int)artistId, model.Amount, model.BankName, model.AccountHolder, model.AccountNumber
            );
            if (!result)
                return BadRequest("Insufficient balance or wallet not found.");
            return Ok(new { success = true, message = "Withdraw request created successfully." });
        }

        // 2. Xem số dư ví
        [HttpGet("balance")]
        public async Task<IActionResult> GetWalletBalance()
        {
            var accountId = GetCurrentAccountId();
            var artistId = await _artistProfileService.GetArtistIdByAccountIdAsync(accountId);
            if (artistId == null)
                return NotFound("Artist profile not found for this account.");

            var result = await _artistProfileService.GetWalletBalanceAsync(artistId.Value);
            return StatusCode(result.StatusCode, result);
        }

        // 3. Xem danh sách hoa hồng
        [HttpGet("commissions")]
        public async Task<IActionResult> GetMyCommissions()
        {
            var accountId = GetCurrentAccountId();
            var artistId = await _artistProfileService.GetArtistIdByAccountIdAsync(accountId);
            if (artistId == null)
                return NotFound("Artist profile not found for this account.");

            var result = await _artistProfileService.GetMyCommissionsAsync(artistId.Value);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("withdraw-requests")]
        public async Task<IActionResult> GetMyWithdrawRequests()
        {
            var accountId = GetCurrentAccountId();
            var artistId = await _artistProfileService.GetArtistIdByAccountIdAsync(accountId);
            if (artistId == null)
                return NotFound("Artist profile not found for this account.");

            var result = await _artistProfileService.GetMyWithdrawRequestsAsync(artistId.Value);
            return StatusCode(result.StatusCode, result);
        }

        // Xem danh sách đơn hàng đã bán
        [HttpGet("my-orders")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null)
        {
            var accountId = GetCurrentAccountId();
            var artistId = await _artistProfileService.GetArtistIdByAccountIdAsync(accountId);
            if (artistId == null)
                return NotFound("Artist profile not found for this account.");

            var result = await _artistProfileService.GetMyOrdersAsync(artistId.Value, page, size, searchTerm, status);
            return Ok(result);
        }

        // Xem chi tiết 1 đơn hàng
        [HttpGet("orders/{orderId}")]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> GetOrderDetail(int orderId)
        {
            var accountId = GetCurrentAccountId();
            var artistId = await _artistProfileService.GetArtistIdByAccountIdAsync(accountId);
            if (artistId == null)
                return NotFound("Artist profile not found for this account.");

            var result = await _artistProfileService.GetOrderDetailAsync(artistId.Value, orderId);
            return StatusCode(result.StatusCode, result);
        }
    }

    public class CreateWithdrawRequestModel
    {
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        public string AccountHolder { get; set; }
        public string AccountNumber { get; set; }
    }
}
