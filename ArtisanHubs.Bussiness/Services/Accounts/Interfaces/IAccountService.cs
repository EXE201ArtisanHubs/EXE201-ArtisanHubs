
using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.DTOs.DTO.Reponse.Accounts;
using ArtisanHubs.DTOs.DTO.Request.Accounts;
using ArtisanHubs.DTOs.DTOs.Reponse;
using ArtisanHubs.DTOs.DTOs.Request.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Accounts.Interfaces
{
    public interface IAccountService
    {
        Task<ApiResponse<IPaginate<Account>>> GetAllAccountAsync(int page, int size, string? searchTerm = null);
        Task<ApiResponse<AccountResponse?>> GetByIdAsync(int id);
        Task<ApiResponse<AccountResponse>> CreateAsync(AccountRequest request, string? avatarUrl = null);
        Task<ApiResponse<AccountResponse?>> UpdateAsync(int id, AccountRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<LoginResponse?>> LoginAsync(LoginRequest request);
        Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ApiResponse<LoginResponse>> LoginWithGoogleAsync(GoogleLoginRequest request);
    }
}
