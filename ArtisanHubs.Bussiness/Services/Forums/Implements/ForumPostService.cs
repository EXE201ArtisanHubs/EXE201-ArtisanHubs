using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.Forums.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Forums.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.Forums;
using ArtisanHubs.DTOs.DTO.Request.Forums;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Forums.Implements
{
    public class ForumPostService : IForumPostService
    {
        private readonly IForumPostRepository _forumPostRepository;
        private readonly IForumThreadRepository _threadRepo;
        private readonly IMapper _mapper;
        public ForumPostService(IForumPostRepository forumPostRepository, IMapper mapper, IForumThreadRepository threadRepo)
        {
            _forumPostRepository = forumPostRepository;
            _mapper = mapper;
            _threadRepo = threadRepo;
        }

        public async Task<ApiResponse<ForumPostResponse>> CreatePostAsync(CreateForumPostRequest request, int authorId)
        {
            try
            {
                // Kiểm tra xem bài đăng mà người dùng muốn trả lời có tồn tại không
                var threadExists = await _forumPostRepository.CheckIfThreadExistsAsync(request.ForumThreadId);
                if (!threadExists)
                {
                    return ApiResponse<ForumPostResponse>.FailResponse("Thread not found.", 404);
                }

                var postEntity = _mapper.Map<ForumPost>(request);
                postEntity.AuthorId = authorId;

                await _forumPostRepository.CreateAsync(postEntity);

                // Lấy lại thông tin post cùng với author để trả về cho client
                var createdPost = await _forumPostRepository.GetPostWithAuthorAsync(postEntity.Id); // Bạn sẽ cần tạo hàm này
                var response = _mapper.Map<ForumPostResponse>(createdPost);

                return ApiResponse<ForumPostResponse>.SuccessResponse(response, "Post created successfully.", 201);
            }
            catch (Exception ex)
            {
                return ApiResponse<ForumPostResponse>.FailResponse(ex.Message, 500);
            }
        }

        public async Task<ApiResponse<bool>> DeletePostAsync(int postId, int authorId)
        {
            try
            {
                var postDelete = await _forumPostRepository.GetByIdAsync(postId);
                if (postDelete == null)
                {
                    return ApiResponse<bool>.FailResponse("Post not found.", 404);
                }

                // Logic quyền: chỉ tác giả hoặc Admin mới được xóa
                // (Ở đây ví dụ chỉ tác giả được xóa)
                if (postDelete.AuthorId != authorId)
                {
                    return ApiResponse<bool>.FailResponse("You are not authorized to delete this post.", 403);
                }

                await _forumPostRepository.RemoveAsync(postDelete);

                return ApiResponse<bool>.SuccessResponse(true, "Post deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailResponse(ex.Message, 500);
            }
        }
    }
}
