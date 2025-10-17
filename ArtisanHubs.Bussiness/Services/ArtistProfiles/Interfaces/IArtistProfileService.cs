using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Request.ArtistProfile;

namespace ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces
{
    public interface IArtistProfileService
    {
        Task<ApiResponse<ArtistProfileResponse>> GetMyProfileAsync(int accountId);
        Task<ApiResponse<ArtistProfileResponse?>> UpdateMyProfileAsync(int accountId, ArtistProfileRequest request);
        Task<ApiResponse<ArtistProfileResponse>> CreateMyProfileAsync(int accountId, ArtistProfileRequest request);
        Task<ApiResponse<IPaginate<Artistprofile>>> GetAllProfilesAsync(int page, int size, string? searchTerm = null);
        Task<ApiResponse<bool>> DeleteProfileAsync(int id);
        //Task<ApiResponse<IEnumerable<ArtistProfileResponse>>> GetAllArtistsAsync();
    }
}
