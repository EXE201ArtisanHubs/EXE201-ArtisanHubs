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
    public interface IForumThreadService
    {
        Task<ApiResponse<IEnumerable<ForumThreadResponse>>> GetThreadsByTopicAsync(int topicId);
        Task<ApiResponse<ForumThreadResponse?>> GetThreadByIdAsync(int threadId);
        Task<ApiResponse<ForumThreadResponse>> CreateThreadAsync(CreateForumThreadRequest request, int authorId);
        Task<ApiResponse<ForumThreadResponse?>> UpdateThreadAsync(int threadId, UpdateForumThreadRequest request, int authorId);
        Task<ApiResponse<bool>> DeleteThreadAsync(int threadId, int authorId);
    }
}
