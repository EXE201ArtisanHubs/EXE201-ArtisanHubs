using ArtisanHubs.Bussiness.Services.Forums.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.Forums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtisanHubs.API.Controllers
{
    [ApiController]
    [Route("api/v1/forum-posts")]
    public class ForumPostsController : ControllerBase
    {
        private readonly IForumPostService _postService;

        public ForumPostsController(IForumPostService postService)
        {
            _postService = postService;
        }

        private int GetCurrentAccountId()
        {
            var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountIdString))
            {
                throw new InvalidOperationException("Account ID claim (NameIdentifier) not found in token.");
            }
            return int.Parse(accountIdString);
        }

        /// <summary>
        /// Tạo một bình luận/trả lời mới trong một bài đăng (thread)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreateForumPostRequest request)
        {
            var authorId = GetCurrentAccountId();
            var result = await _postService.CreatePostAsync(request, authorId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{postId}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var authorId = GetCurrentAccountId();
            var result = await _postService.DeletePostAsync(postId, authorId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
