using ArtisanHubs.Bussiness.Services.Forums.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.Forums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtisanHubs.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ForumTopicController : ControllerBase
    {
        private readonly IForumTopicService _forumTopicService;

        public ForumTopicController(IForumTopicService forumTopicService)
        {
            _forumTopicService = forumTopicService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTopics()
        {
            var result = await _forumTopicService.GetAllTopicsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{topicId}")]
        public async Task<IActionResult> GetTopicById(int topicId)
        {
            var result = await _forumTopicService.GetTopicByIdAsync(topicId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateTopic([FromBody] CreateForumTopicRequest request)
        {
            var result = await _forumTopicService.CreateTopicAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{topicId}")]
        public async Task<IActionResult> UpdateTopic(int topicId, [FromBody] UpdateForumTopicRequest request)
        {
            var result = await _forumTopicService.UpdateTopicAsync(topicId, request);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{topicId}")]
        public async Task<IActionResult> DeleteTopic(int topicId)
        {
            var result = await _forumTopicService.DeleteTopicAsync(topicId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
