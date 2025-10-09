using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.DTOs.DTO.Reponse.Forums;
using ArtisanHubs.DTOs.DTO.Request.Forums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Forums.Interfaces
{
    public interface IForumTopicService
    {
        Task<ApiResponse<IEnumerable<ForumTopicResponse>>> GetAllTopicsAsync();
        Task<ApiResponse<ForumTopicResponse?>> GetTopicByIdAsync(int topicId);
        Task<ApiResponse<ForumTopicResponse>> CreateTopicAsync(CreateForumTopicRequest request);
        Task<ApiResponse<ForumTopicResponse?>> UpdateTopicAsync(int topicId, UpdateForumTopicRequest request);
        Task<ApiResponse<bool>> DeleteTopicAsync(int topicId);
    }
}
