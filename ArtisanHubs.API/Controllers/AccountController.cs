using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.Accounts.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.Accounts;
using ArtisanHubs.DTOs.DTOs.Reponse;
using ArtisanHubs.DTOs.DTOs.Request.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtisanHubs.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
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

        /// <summary>
        /// Lấy danh sách tất cả tài khoản
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _accountService.GetAllAccountAsync();
            return StatusCode(result.StatusCode, result); ;
        }

        /// <summary>
        /// Lấy 1 tài khoản theo id
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _accountService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Tạo mới tài khoản
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AccountRequest request)
        {
            var result = await _accountService.CreateAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Cập nhật tài khoản
        /// </summary>
        [Authorize(Roles = "Customer,Artist,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AccountRequest request)
        {
            var result = await _accountService.UpdateAsync(id, request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Xóa tài khoản
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _accountService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _accountService.LoginAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Bắt đầu quy trình quên mật khẩu
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _accountService.ForgotPasswordAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Hoàn tất quy trình quên mật khẩu với token và mật khẩu mới
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _accountService.ResetPasswordAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var result = await _accountService.LoginWithGoogleAsync(request);
            return StatusCode(result.StatusCode, result);
        }
    }
}
