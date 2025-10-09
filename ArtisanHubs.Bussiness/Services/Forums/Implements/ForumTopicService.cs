using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.Forums.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Forums.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.Forums;
using ArtisanHubs.DTOs.DTO.Request.Forums;
using ArtisanHubs.DTOs.DTOs.Reponse;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Forums.Implements
{
    public class ForumTopicService : IForumTopicService
    {
        private readonly IForumTopicRepository _forumTopicRepository;
        private readonly IMapper _mapper;
        private readonly IForumThreadRepository _threadRepo;
        public ForumTopicService(IForumTopicRepository forumTopicRepository, IMapper mapper, IForumThreadRepository threadRepo)
        {
            _forumTopicRepository = forumTopicRepository;
            _mapper = mapper;
            _threadRepo = threadRepo;
        }

        public async Task<ApiResponse<IEnumerable<ForumTopicResponse>>> GetAllTopicsAsync()
        {
            try
            {
                var topic = await _forumTopicRepository.GetAllAsync();

                if (topic == null || !topic.Any())
                {
                    return ApiResponse<IEnumerable<ForumTopicResponse>>.FailResponse("No forum topics found.", 404);
                }

                var response = _mapper.Map<IEnumerable<ForumTopicResponse>>(topic);

                return ApiResponse<IEnumerable<ForumTopicResponse>>.SuccessResponse(response, "Get all Forum Topic successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<ForumTopicResponse>>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<ForumTopicResponse?>> GetTopicByIdAsync(int topicId)
        {
            try
            {
                var topic = await _forumTopicRepository.GetByIdAsync(topicId);
                if (topic == null)
                {
                    return ApiResponse<ForumTopicResponse?>.FailResponse("Topic not found.", 404);
                }
                var response = _mapper.Map<ForumTopicResponse>(topic);
                return ApiResponse<ForumTopicResponse?>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<ForumTopicResponse?>.FailResponse(ex.Message, 500);
            }
        }

        // === CREATE ===
        public async Task<ApiResponse<ForumTopicResponse>> CreateTopicAsync(CreateForumTopicRequest request)
        {
            try
            {
                if (await _forumTopicRepository.ExistsByTitleAsync(request.Title))
                {
                    return ApiResponse<ForumTopicResponse>.FailResponse("A topic with this title already exists.", 400);
                }

                var topicEntity = _mapper.Map<ForumTopic>(request);
                await _forumTopicRepository.CreateAsync(topicEntity);
                
                var response = _mapper.Map<ForumTopicResponse>(topicEntity);
                return ApiResponse<ForumTopicResponse>.SuccessResponse(response, "Topic created successfully.", 201);
            }
            catch (Exception ex)
            {
                return ApiResponse<ForumTopicResponse>.FailResponse(ex.Message, 500);
            }
        }

        // === UPDATE ===
        public async Task<ApiResponse<ForumTopicResponse?>> UpdateTopicAsync(int topicId, UpdateForumTopicRequest request)
        {
            try
            {
                var existingTopic = await _forumTopicRepository.GetByIdAsync(topicId);
                if (existingTopic == null)
                {
                    return ApiResponse<ForumTopicResponse?>.FailResponse("Topic not found.", 404);
                }

                _mapper.Map(request, existingTopic);
                _forumTopicRepository.UpdateAsync(existingTopic);

                var response = _mapper.Map<ForumTopicResponse>(existingTopic);
                return ApiResponse<ForumTopicResponse?>.SuccessResponse(response, "Topic updated successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<ForumTopicResponse?>.FailResponse(ex.Message, 500);
            }
        }

        // === DELETE ===
        public async Task<ApiResponse<bool>> DeleteTopicAsync(int topicId)
        {
            try
            {
                var topicToDelete = await _forumTopicRepository.GetByIdAsync(topicId);
                if (topicToDelete == null)
                {
                    return ApiResponse<bool>.FailResponse("Topic not found.", 404);
                }

                // Logic nghiệp vụ: Không cho xóa topic nếu nó đã có thread bên trong
                var hasThreads = await _threadRepo.HasThreadsInTopicAsync(topicId);
                if (hasThreads)
                {
                    return ApiResponse<bool>.FailResponse("Cannot delete this topic because it contains threads.", 400);
                }

                _forumTopicRepository.RemoveAsync(topicToDelete);

                return ApiResponse<bool>.SuccessResponse(true, "Topic deleted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailResponse(ex.Message, 500);
            }
        }
    }
}
