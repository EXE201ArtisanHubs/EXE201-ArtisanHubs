using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Accounts.Interfaces;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Request.ArtistProfile;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        private readonly IAccountRepository _accountRepo;
        private readonly ArtisanHubsDbContext _context;
        private readonly PhotoService _photoService;

        public ArtistProfileService(IArtistProfileRepository repo, IMapper mapper, PhotoService photoService, IAccountRepository accountRepository, ArtisanHubsDbContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _photoService = photoService;
            _accountRepo = accountRepository;
            _context = context;
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
            await using var transaction = await _context.Database.BeginTransactionAsync();           
            try
            {
                // Kiểm tra các điều kiện như cũ
                var existingProfile = await _repo.GetProfileByAccountIdAsync(accountId);
                if (existingProfile != null)
                {
                    return ApiResponse<ArtistProfileResponse>.FailResponse("Artist profile already exists for this account.", 409);
                }

                var accountToUpdate = await _accountRepo.GetByIdAsync(accountId);
                if (accountToUpdate == null || accountToUpdate.Role != "Customer")
                {
                    return ApiResponse<ArtistProfileResponse>.FailResponse("Only accounts with 'Customer' role can create an artist profile.", 403);
                }

                // 1. Dùng Repository để tạo profile.
                // Repository này sẽ gọi SaveChanges() lần 1.
                var entity = _mapper.Map<Artistprofile>(request);
                entity.AccountId = accountId;
                entity.CreatedAt = DateTime.UtcNow;
                await _repo.CreateAsync(entity);

                // 2. Dùng Repository để cập nhật role.
                // Repository này sẽ gọi SaveChanges() lần 2.
                accountToUpdate.Role = "Artist";
                await _accountRepo.UpdateAsync(accountToUpdate);

                // Handle image upload
                if (request.ProfileImage != null)
                {
                    var imageUrl = await _photoService.UploadImageAsync(request.ProfileImage);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        entity.ProfileImage = imageUrl; // Adjust property name as needed
                    }
                }

                await _repo.CreateAsync(entity);

                var response = _mapper.Map<ArtistProfileResponse>(entity);
                return ApiResponse<ArtistProfileResponse>.SuccessResponse(response, "Create profile and upgrade role to Artist successfully", 201);
            }
            catch (Exception ex)
            {
                // 4. Nếu có bất kỳ lỗi nào, rollback tất cả thay đổi
                await transaction.RollbackAsync();
                return ApiResponse<ArtistProfileResponse>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Cập nhật profile cho nghệ nhân
        public async Task<ApiResponse<ArtistProfileResponse?>> UpdateMyProfileAsync(int accountId, ArtistProfileRequest request)
        {
            try
            {
                // Thay vì GetByAccountIdAsync, ta dùng GetQueryable để Include
                var existing = await _repo.GetQueryable()
                                          .Include(p => p.Achievements)
                                          .FirstOrDefaultAsync(p => p.AccountId == accountId);

                if (existing == null)
                    return ApiResponse<ArtistProfileResponse?>.FailResponse("Artist profile not found to update", 404);

                // Mapper sẽ cập nhật các trường cơ bản và xử lý danh sách Achievements
                _mapper.Map(request, existing);

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

        //public async Task<ApiResponse<IEnumerable<ArtistProfileResponse>>> GetAllArtistsAsync()
        //{
        //    try
        //    {
        //        var artists = await _repo.GetAllArtistsAsync();
        //        var response = _mapper.Map<IEnumerable<ArtistProfileResponse>>(artists);

        //        return ApiResponse<IEnumerable<ArtistProfileResponse>>.SuccessResponse(
        //            response, "Get all artists successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ApiResponse<IEnumerable<ArtistProfileResponse>>.FailResponse(
        //            $"An error occurred: {ex.Message}", 500);
        //    }
        //}
    }
}
