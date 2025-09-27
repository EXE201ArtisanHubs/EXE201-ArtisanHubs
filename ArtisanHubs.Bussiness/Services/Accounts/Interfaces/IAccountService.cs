
using ArtisanHubs.API.DTOs.Common;
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
        Task<ApiResponse<IEnumerable<AccountResponse>>> GetAllAccountAsync();
        Task<ApiResponse<AccountResponse?>> GetByIdAsync(int id);
        Task<ApiResponse<AccountResponse>> CreateAsync(AccountRequest request);
        Task<ApiResponse<AccountResponse?>> UpdateAsync(int id, AccountRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<AccountResponse>> RegisterAsync(RegisterRequest request);
        Task<ApiResponse<LoginResponse?>> LoginAsync(LoginRequest request);
    }
}
