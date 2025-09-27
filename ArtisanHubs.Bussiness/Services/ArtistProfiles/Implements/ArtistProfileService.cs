using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Request.ArtistProfile;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.ArtistProfiles.Implements
{
    public class ArtistProfileService : IArtistProfileService
    {
        private readonly IArtistProfileRepository _repo;
        private readonly IMapper _mapper;

        public ArtistProfileService(IArtistProfileRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // Lấy profile của nghệ nhân đang đăng nhập
        public async Task<ApiResponse<ArtistProfileResponse?>> GetMyProfileAsync(int accountId)
        {
            try
            {
                var profile = await _repo.GetProfileByAccountIdAsync(accountId);
                if (profile == null)
                    return ApiResponse<ArtistProfileResponse?>.FailResponse("Artist profile not found", 404);

                var response = _mapper.Map<ArtistProfileResponse>(profile);
                return ApiResponse<ArtistProfileResponse?>.SuccessResponse(response, "Get profile successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ArtistProfileResponse?>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Tạo mới profile cho nghệ nhân
        public async Task<ApiResponse<ArtistProfileResponse>> CreateMyProfileAsync(int accountId, ArtistProfileRequest request)
        {
            try
            {
                // Kiểm tra xem profile đã tồn tại chưa
                var existingProfile = await _repo.GetProfileByAccountIdAsync(accountId);
                if (existingProfile != null)
                {
                    // Dùng mã lỗi 409 Conflict vì tài nguyên đã tồn tại
                    return ApiResponse<ArtistProfileResponse>.FailResponse("Artist profile already exists for this account.", 409);
                }

                var entity = _mapper.Map<Artistprofile>(request);
                entity.AccountId = accountId;
                entity.CreatedAt = DateTime.UtcNow; // Gán thời gian tạo

                await _repo.CreateAsync(entity);

                var response = _mapper.Map<ArtistProfileResponse>(entity);
                return ApiResponse<ArtistProfileResponse>.SuccessResponse(response, "Create profile successfully", 201);
            }
            catch (Exception ex)
            {
                return ApiResponse<ArtistProfileResponse>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Cập nhật profile cho nghệ nhân
        public async Task<ApiResponse<ArtistProfileResponse?>> UpdateMyProfileAsync(int accountId, ArtistProfileRequest request)
        {
            try
            {
                var existing = await _repo.GetProfileByAccountIdAsync(accountId);
                if (existing == null)
                    return ApiResponse<ArtistProfileResponse?>.FailResponse("Artist profile not found to update", 404);

                _mapper.Map(request, existing);
                // Bạn có thể thêm các trường cập nhật khác ở đây nếu cần, ví dụ:
                // existing.UpdatedAt = DateTime.UtcNow;

                await _repo.UpdateAsync(existing);

                var response = _mapper.Map<ArtistProfileResponse>(existing);
                return ApiResponse<ArtistProfileResponse?>.SuccessResponse(response, "Update profile successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ArtistProfileResponse?>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Lấy profile tất cả nghệ nhân
        public async Task<ApiResponse<IEnumerable<ArtistProfileResponse>>> GetAllProfilesAsync()
        {
            try
            {
               var profiles = await _repo.GetAllAsync();
                var response = _mapper.Map<IEnumerable<ArtistProfileResponse>>(profiles);
                return ApiResponse<IEnumerable<ArtistProfileResponse>>.SuccessResponse(response, "Get all profiles successfully");
            }
            catch (Exception ex)
            {
               return ApiResponse<IEnumerable<ArtistProfileResponse>>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteProfileAsync(int id)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing == null)
                    return ApiResponse<bool>.FailResponse("Artist profile not found to delete", 404);

               var response = await _repo.RemoveAsync(existing);
                return ApiResponse<bool>.SuccessResponse(true, "Delete profile successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailResponse($"Error: {ex.Message}", 500);
            }
        }
    }
}
