using ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces;
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

        public ArtistProfilesController(IArtistProfileService artistProfileService)
        {
            _artistProfileService = artistProfileService;
        }

        //private int GetCurrentAccountId()
        //{
        //    // User.FindFirstValue là một phương thức tiện ích để tìm claim đầu tiên
        //    // có loại (type) được chỉ định và trả về giá trị của nó.
        //    var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    // Luôn kiểm tra để đảm bảo claim tồn tại trước khi sử dụng
        //    if (string.IsNullOrEmpty(accountIdString))
        //    {
        //        // Lỗi này xảy ra nếu token hợp lệ nhưng lại thiếu claim ID của người dùng,
        //        // cho thấy có vấn đề ở khâu tạo token.
        //        throw new InvalidOperationException("Account ID claim (NameIdentifier) not found in token.");
        //    }

        //    return int.Parse(accountIdString);
        //}
        private int GetCurrentAccountId()
        {
#if DEBUG
            // Giả lập rằng nghệ nhân có AccountId = 4 đang đăng nhập
            // Đảm bảo trong database của bạn có Account với ID=4 và Role="Artist"
            return 4;
#else
        // Code này sẽ chạy khi deploy production
        var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(accountIdString))
        {
            throw new InvalidOperationException("Account ID claim (NameIdentifier) not found in token.");
        }
        return int.Parse(accountIdString);
#endif
        }

        [HttpGet("me")]
        //[Authorize(Roles = "Artist")]
        public async Task<IActionResult> GetMyProfile()
        {
            var accountId = GetCurrentAccountId();
            var result = await _artistProfileService.GetMyProfileAsync(accountId);

            return StatusCode(result.StatusCode, result);
        }

        // POST: api/artist-profiles/me
        // Tạo profile cho chính nghệ nhân đang đăng nhập
        [HttpPost("me")]
        //[Authorize(Roles = "Artist")]
        public async Task<IActionResult> CreateMyProfile([FromBody] ArtistProfileRequest request)
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
        [HttpPut("me")]
        //[Authorize(Roles = "Artist")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] ArtistProfileRequest request)
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
        [HttpGet]
        public async Task<IActionResult> GetAllProfiles()
        {
            var result = await _artistProfileService.GetAllProfilesAsync();
            return StatusCode(result.StatusCode, result);
        }

        // Xóa một profile (chức năng này thường dành cho Admin)
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            var result = await _artistProfileService.DeleteProfileAsync(id);
            return StatusCode(result.StatusCode, result);
        }

    }
}
