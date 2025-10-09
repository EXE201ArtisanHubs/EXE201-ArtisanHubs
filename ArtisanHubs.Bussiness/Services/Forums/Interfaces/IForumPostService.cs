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
    public interface IForumPostService
    {
        Task<ApiResponse<ForumPostResponse>> CreatePostAsync(CreateForumPostRequest request, int authorId);
        Task<ApiResponse<bool>> DeletePostAsync(int postId, int authorId);
    }
}
