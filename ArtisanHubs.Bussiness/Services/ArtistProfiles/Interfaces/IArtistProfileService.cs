using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Reponse.Order;
using ArtisanHubs.DTOs.DTO.Request.ArtistProfile;

namespace ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces
{
    public interface IArtistProfileService
    {
        Task<int?> GetArtistIdByAccountIdAsync(int accountId);
        Task<ApiResponse<ArtistProfileResponse>> GetMyProfileAsync(int accountId);
        Task<ApiResponse<ArtistProfileResponse?>> UpdateMyProfileAsync(int accountId, ArtistProfileRequest request);
        Task<ApiResponse<ArtistProfileResponse>> CreateMyProfileAsync(int accountId, ArtistProfileRequest request);
        Task<ApiResponse<IPaginate<Artistprofile>>> GetAllProfilesAsync(int page, int size, string? searchTerm = null);
        Task<ApiResponse<bool>> DeleteProfileAsync(int id);
        Task<bool> CreateWithdrawRequestAsync(int artistId, decimal amount, string bankName, string accountHolder, string accountNumber);
        Task<ApiResponse<decimal>> GetWalletBalanceAsync(int artistId);
        Task<ApiResponse<List<Commission>>> GetMyCommissionsAsync(int artistId);
        Task<ApiResponse<List<Withdrawrequest>>> GetMyWithdrawRequestsAsync(int artistId);
        Task<ApiResponse<IPaginate<ArtistOrderResponse>>> GetMyOrdersAsync(int artistId, int page, int size, string searchTerm, string status);
        Task<ApiResponse<ArtistOrderResponse>> GetOrderDetailAsync(int artistId, int orderId);
    }
}
