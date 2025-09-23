using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.Accounts.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Accounts.Implements;
using ArtisanHubs.Data.Repositories.Accounts.Interfaces;
using ArtisanHubs.DTOs.DTOs.Reponse;
using ArtisanHubs.DTOs.DTOs.Request.Accounts;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Accounts.Implements
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repo;
        private readonly IMapper _mapper;

        public AccountService(IAccountRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // Lấy tất cả Account
        public async Task<ApiResponse<IEnumerable<AccountResponse>>> GetAllAccountAsync()
        {
            try
            {
                var accounts = await _repo.GetAllAsync();
                var response = _mapper.Map<IEnumerable<AccountResponse>>(accounts);

                return ApiResponse<IEnumerable<AccountResponse>>.SuccessResponse(response, "Get all accounts successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<AccountResponse>>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Lấy Account theo Id
        public async Task<ApiResponse<AccountResponse?>> GetByIdAsync(int id)
        {
            try
            {
                var account = await _repo.GetByIdAsync(id);
                if (account == null)
                    return ApiResponse<AccountResponse?>.FailResponse("Account not found", 404);

                var response = _mapper.Map<AccountResponse>(account);
                return ApiResponse<AccountResponse?>.SuccessResponse(response, "Get account successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<AccountResponse?>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Tạo mới Account
        public async Task<ApiResponse<AccountResponse>> CreateAsync(AccountRequest request)
        {
            try
            {
                var entity = _mapper.Map<Account>(request);
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = "Active";

                await _repo.CreateAsync(entity);

                var response = _mapper.Map<AccountResponse>(entity);
                return ApiResponse<AccountResponse>.SuccessResponse(response, "Create account successfully", 201);
            }
            catch (Exception ex)
            {
                return ApiResponse<AccountResponse>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Cập nhật Account
        public async Task<ApiResponse<AccountResponse?>> UpdateAsync(int id, AccountRequest request)
        {
            try
            {
                var existing = await _repo.GetByIdAsync(id);
                if (existing == null) return ApiResponse<AccountResponse?>.FailResponse("Account not found", 404);

                _mapper.Map(request, existing);
                existing.UpdatedAt = DateTime.UtcNow;

                await _repo.UpdateAsync(existing);

                var response = _mapper.Map<AccountResponse>(existing);
                return ApiResponse<AccountResponse?>.SuccessResponse(response, "Update account successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<AccountResponse?>.FailResponse($"Error: {ex.Message}", 500);
            }
        }

        // Xoá Account
        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var account = await _repo.GetByIdAsync(id);
                if (account == null) return ApiResponse<bool>.FailResponse("Account not found", 404);

                await _repo.RemoveAsync(account);
                return ApiResponse<bool>.SuccessResponse(true, "Delete account successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailResponse($"Error: {ex.Message}", 500);
            }
        }
    }
}