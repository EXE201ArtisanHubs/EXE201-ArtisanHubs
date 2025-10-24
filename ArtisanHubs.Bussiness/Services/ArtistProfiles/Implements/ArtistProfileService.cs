using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.Data.Repositories.Accounts.Interfaces;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Request.ArtistProfile;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

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

        //// Tạo mới profile cho nghệ nhân
        //public async Task<ApiResponse<ArtistProfileResponse>> CreateMyProfileAsync(int accountId, ArtistProfileRequest request)
        //{
        //    await using var transaction = await _context.Database.BeginTransactionAsync();           
        //    try
        //    {
        //        // Kiểm tra các điều kiện như cũ
        //        var existingProfile = await _repo.GetProfileByAccountIdAsync(accountId);
        //        if (existingProfile != null)
        //        {
        //            return ApiResponse<ArtistProfileResponse>.FailResponse("Artist profile already exists for this account.", 409);
        //        }

        //        var accountToUpdate = await _accountRepo.GetByIdAsync(accountId);
        //        if (accountToUpdate == null || accountToUpdate.Role != "Customer")
        //        {
        //            return ApiResponse<ArtistProfileResponse>.FailResponse("Only accounts with 'Customer' role can create an artist profile.", 403);
        //        }

        //        // 1. Dùng Repository để tạo profile.
        //        // Repository này sẽ gọi SaveChanges() lần 1.
        //        var entity = _mapper.Map<Artistprofile>(request);
        //        entity.AccountId = accountId;
        //        entity.CreatedAt = DateTime.UtcNow;
        //        await _repo.CreateAsync(entity);

        //        // 2. Dùng Repository để cập nhật role.
        //        // Repository này sẽ gọi SaveChanges() lần 2.
        //        accountToUpdate.Role = "Artist";
        //        await _accountRepo.UpdateAsync(accountToUpdate);

        //        // Handle image upload
        //        if (request.ProfileImage != null)
        //        {
        //            var imageUrl = await _photoService.UploadImageAsync(request.ProfileImage);
        //            if (!string.IsNullOrEmpty(imageUrl))
        //            {
        //                entity.ProfileImage = imageUrl; // Adjust property name as needed
        //            }
        //        }

        //        await _repo.CreateAsync(entity);

        //        var response = _mapper.Map<ArtistProfileResponse>(entity);
        //        return ApiResponse<ArtistProfileResponse>.SuccessResponse(response, "Create profile and upgrade role to Artist successfully", 201);
        //    }
        //    catch (Exception ex)
        //    {
        //        // 4. Nếu có bất kỳ lỗi nào, rollback tất cả thay đổi
        //        await transaction.RollbackAsync();
        //        return ApiResponse<ArtistProfileResponse>.FailResponse($"Error: {ex.Message}", 500);
        //    }
        //}
        public async Task<ApiResponse<ArtistProfileResponse>> CreateMyProfileAsync(int accountId, ArtistProfileRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingProfile = await _repo.GetProfileByAccountIdAsync(accountId);
                if (existingProfile != null)
                    return ApiResponse<ArtistProfileResponse>.FailResponse("Artist profile already exists for this account.", 409);

                var accountToUpdate = await _accountRepo.GetByIdAsync(accountId);
                if (accountToUpdate == null || accountToUpdate.Role != "Customer")
                    return ApiResponse<ArtistProfileResponse>.FailResponse("Only accounts with 'Customer' role can create an artist profile.", 403);

                // 1️⃣ Tạo profile mới
                var entity = _mapper.Map<Artistprofile>(request);
                entity.AccountId = accountId;
                entity.CreatedAt = DateTime.UtcNow;

                await _repo.CreateAsync(entity); // ✅ chỉ gọi 1 lần

                // 2️⃣ Cập nhật role
                accountToUpdate.Role = "Artist";
                await _accountRepo.UpdateAsync(accountToUpdate);

                // 3️⃣ Upload ảnh (cập nhật sau khi có URL)
                if (request.ProfileImage != null)
                {
                    var imageUrl = await _photoService.UploadImageAsync(request.ProfileImage);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        entity.ProfileImage = imageUrl;
                        await _repo.UpdateAsync(entity); // chỉ update, không create lại
                    }
                }

                var response = _mapper.Map<ArtistProfileResponse>(entity);
                await transaction.CommitAsync();
                return ApiResponse<ArtistProfileResponse>.SuccessResponse(response, "Create profile and upgrade role to Artist successfully", 201);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse<ArtistProfileResponse>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Cập nhật profile cho nghệ nhân
        // Cập nhật profile cho nghệ nhân
        public async Task<ApiResponse<ArtistProfileResponse?>> UpdateMyProfileAsync(int accountId, ArtistProfileRequest request)
        {
            try
            {
                var existing = await _repo.GetQueryable()
                                          .Include(p => p.Achievements)
                                          .FirstOrDefaultAsync(p => p.AccountId == accountId);

                if (existing == null)
                    return ApiResponse<ArtistProfileResponse?>.FailResponse("Artist profile not found to update", 404);

                // Mapper sẽ cập nhật các trường cơ bản và xử lý danh sách Achievements
                _mapper.Map(request, existing);

                    if (request.ProfileImage != null)
                    {
                        // Upload the image and get the URL
                        var imageUrl = await _photoService.UploadImageAsync(request.ProfileImage);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            existing.ProfileImage = imageUrl; // Assign the uploaded image URL to the profile
                        }
                    }

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
        public async Task<ApiResponse<IPaginate<Artistprofile>>> GetAllProfilesAsync(int page, int size, string? searchTerm = null)
        {
            try
            {
                // Lấy danh sách có phân trang
                var result = await _repo.GetPagedAsync(null, page, size, searchTerm);

                // ✅ Trả về gói trong ApiResponse mà KHÔNG ép kiểu
                return ApiResponse<IPaginate<Artistprofile>>.SuccessResponse(
                    result,
                    "Get paginated accounts successfully"
                );
            }
            catch (Exception ex)
            {
                // ✅ Bắt lỗi và trả về fail response
                return ApiResponse<IPaginate<Artistprofile>>.FailResponse($"Error: {ex.Message}");
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

        public async Task<bool> CreateWithdrawRequestAsync(int artistId, decimal amount, string bankName, string accountHolder, string accountNumber)
        {
            var wallet = await _context.Artistwallets.FirstOrDefaultAsync(w => w.ArtistId == artistId);
            if (wallet == null || wallet.Balance < amount)
                return false;

            var withdrawRequest = new Withdrawrequest
            {
                ArtistId = artistId,
                Amount = amount,
                BankName = bankName,
                AccountHolder = accountHolder,
                AccountNumber = accountNumber,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };
            _context.Withdrawrequests.Add(withdrawRequest);

            // Có thể trừ PendingBalance nếu muốn giữ tiền chờ duyệt
            wallet.PendingBalance += amount;
            wallet.CreatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Lấy số dư ví của nghệ nhân
        public async Task<ApiResponse<decimal>> GetWalletBalanceAsync(int artistId)
        {
            var wallet = await _context.Artistwallets.FirstOrDefaultAsync(w => w.ArtistId == artistId);
            if (wallet == null)
                return ApiResponse<decimal>.FailResponse("Wallet not found", 404);
            return ApiResponse<decimal>.SuccessResponse(wallet.Balance, "Get wallet balance successfully");
        }

        // Lấy danh sách hoa hồng của nghệ nhân
        public async Task<ApiResponse<List<Commission>>> GetMyCommissionsAsync(int artistId)
        {
            var commissions = await _context.Commissions
                .Where(c => c.ArtistId == artistId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return ApiResponse<List<Commission>>.SuccessResponse(commissions, "Get commissions successfully");
        }

        // Lấy danh sách lệnh rút tiền của nghệ nhân
        public async Task<ApiResponse<List<Withdrawrequest>>> GetMyWithdrawRequestsAsync(int artistId)
        {
            var withdraws = await _context.Withdrawrequests
                .Where(w => w.ArtistId == artistId)
                .OrderByDescending(w => w.RequestedAt)
                .ToListAsync();
            return ApiResponse<List<Withdrawrequest>>.SuccessResponse(withdraws, "Get withdraw requests successfully");
        }
    }
}
