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
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Forums.Implements
{
    public class ForumThreadService : IForumThreadService
    {
        private readonly IForumThreadRepository _forumThreadRepo;
        private readonly IForumTopicRepository _topicRepo;
        private readonly IMapper _mapper;

        public ForumThreadService(IForumThreadRepository forumThreadRepo, IMapper mapper, IForumTopicRepository forumTopic)
        {
            _forumThreadRepo = forumThreadRepo;
            _mapper = mapper;
            _topicRepo = forumTopic;
        }

        public async Task<ApiResponse<IEnumerable<ForumThreadResponse>>> GetThreadsByTopicAsync(int topicId)
        {
            try
            {
                var topicExists = await _forumThreadRepo.GetByIdAsync(topicId);
                if (topicExists == null)
                {
                    return ApiResponse<IEnumerable<ForumThreadResponse>>.FailResponse("Forum topic not found.", 404);
                }

                var threads = await _forumThreadRepo.GetThreadsByTopicAsync(topicId); // Cần một hàm custom trong repo để lấy cả author
                var response = _mapper.Map<IEnumerable<ForumThreadResponse>>(threads);
                return ApiResponse<IEnumerable<ForumThreadResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                return ApiResponse<IEnumerable<ForumThreadResponse>>.FailResponse($"An unexpected error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<ForumThreadResponse?>> GetThreadByIdAsync(int threadId)
        {
            try
            {
                var thread = await _forumThreadRepo.GetThreadWithDetailsAsync(threadId); // Hàm này lấy cả posts và author
                if (thread == null)
                {
                    return ApiResponse<ForumThreadResponse?>.FailResponse("Thread not found.", 404);
                }
                var response = _mapper.Map<ForumThreadResponse>(thread);
                return ApiResponse<ForumThreadResponse?>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<ForumThreadResponse?>.FailResponse($"An unexpected error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<ForumThreadResponse>> CreateThreadAsync(CreateForumThreadRequest request, int authorId)
        {
            try
            {
                var topicExists = await _topicRepo.GetByIdAsync(request.ForumTopicId);
                if (topicExists == null)
                {
                    return ApiResponse<ForumThreadResponse>.FailResponse("Forum topic not found.", 400);
                }

                var threadEntity = _mapper.Map<ForumThread>(request);
                threadEntity.AuthorId = authorId;

                var initialPost = new ForumPost
                {
                    Content = request.InitialPostContent,
                    AuthorId = authorId,
                };
                threadEntity.Posts.Add(initialPost);

                await _forumThreadRepo.CreateAsync(threadEntity);
                var createdThreadWithDetails = await _forumThreadRepo.GetThreadWithDetailsAsync(threadEntity.Id);
                var response = _mapper.Map<ForumThreadResponse>(createdThreadWithDetails);
                return ApiResponse<ForumThreadResponse>.SuccessResponse(response, "Thread created successfully.", 201);
            }
            catch (Exception ex)
            {
                return ApiResponse<ForumThreadResponse>.FailResponse(ex.Message, 500);
            }
        }

        public async Task<ApiResponse<ForumThreadResponse?>> UpdateThreadAsync(int threadId, UpdateForumThreadRequest request, int authorId)
        {
            try
            {
                var existingThread = await _forumThreadRepo.GetByIdAsync(threadId);
                if (existingThread == null)
                {
                    return ApiResponse<ForumThreadResponse?>.FailResponse("Thread not found.", 404);
                }

                // Logic quyền: chỉ tác giả mới được sửa bài
                if (existingThread.AuthorId != authorId)
                {
                    return ApiResponse<ForumThreadResponse?>.FailResponse("You are not authorized to update this thread.", 403);
                }

                _mapper.Map(request, existingThread);
               await _forumThreadRepo.UpdateAsync(existingThread);

                var response = _mapper.Map<ForumThreadResponse>(existingThread);
                return ApiResponse<ForumThreadResponse?>.SuccessResponse(response, "Thread updated successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<ForumThreadResponse?>.FailResponse(ex.Message, 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteThreadAsync(int threadId, int authorId)
        {
            try
            {
                var threadToDelete = await _forumThreadRepo.GetByIdAsync(threadId);
                if (threadToDelete == null)
                {
                    return ApiResponse<bool>.FailResponse("Thread not found.", 404);
                }

                // Logic quyền: chỉ tác giả hoặc Admin mới được xóa
                // (Ở đây ví dụ chỉ tác giả được xóa)
                if (threadToDelete.AuthorId != authorId)
                {
                    return ApiResponse<bool>.FailResponse("You are not authorized to delete this thread.", 403);
                }

                await _forumThreadRepo.RemoveAsync(threadToDelete);

                return ApiResponse<bool>.SuccessResponse(true, "Thread deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailResponse(ex.Message, 500);
            }
        }
    }
}
