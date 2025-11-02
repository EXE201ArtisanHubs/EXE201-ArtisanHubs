using ArtisanHubs.Bussiness.Services.Forums.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.Forums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtisanHubs.API.Controllers
{
    [ApiController]
    [Route("api/v1/forum-threads")]
    public class ForumThreadsController : ControllerBase
    {
        private readonly IForumThreadService _threadService;

        public ForumThreadsController(IForumThreadService threadService)
        {
            _threadService = threadService;
        }

        private int GetCurrentAccountId()
        {
            var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountIdString))
            {
                // Lỗi này chỉ xảy ra khi token hợp lệ nhưng thiếu claim ID,
                // cho thấy vấn đề ở khâu tạo token.
                throw new InvalidOperationException("Account ID claim (NameIdentifier) not found in token.");
            }
            return int.Parse(accountIdString);
        }

        /// <summary>
        /// Lấy tất cả bài đăng theo một chuyên mục (topic)
        /// </summary>
        [HttpGet("by-topic/{topicId}")]
        public async Task<IActionResult> GetThreadsByTopic(int topicId)
        {
            var result = await _threadService.GetThreadsByTopicAsync(topicId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy chi tiết một bài đăng (bao gồm các bình luận)
        /// </summary>
        [HttpGet("{threadId}")]
        public async Task<IActionResult> GetThreadById(int threadId)
        {
            var result = await _threadService.GetThreadByIdAsync(threadId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Tạo một bài đăng mới với khả năng upload ảnh
        /// </summary>
        [HttpPost]
        [Authorize] // Yêu cầu phải đăng nhập
        public async Task<IActionResult> CreateThread([FromForm] CreateForumThreadRequest request)
        {
            var authorId = GetCurrentAccountId();
            var result = await _threadService.CreateThreadAsync(request, authorId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Cập nhật một bài đăng
        /// </summary>
        [HttpPut("{threadId}")]
        [Authorize]
        public async Task<IActionResult> UpdateThread(int threadId, [FromBody] UpdateForumThreadRequest request)
        {
            var authorId = GetCurrentAccountId();
            var result = await _threadService.UpdateThreadAsync(threadId, request, authorId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Xóa một bài đăng
        /// </summary>
        [HttpDelete("{threadId}")]
        [Authorize]
        public async Task<IActionResult> DeleteThread(int threadId)
        {
            var authorId = GetCurrentAccountId();
            var result = await _threadService.DeleteThreadAsync(threadId, authorId);
            return StatusCode(result.StatusCode, result);
        }
    }

}

