using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Request.ArtistProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.ArtistProfiles.Interfaces
{
    public interface IArtistProfileService
    {
        Task<ApiResponse<ArtistProfileResponse>> GetMyProfileAsync(int accountId);
        Task<ApiResponse<ArtistProfileResponse?>> UpdateMyProfileAsync(int accountId, ArtistProfileRequest request);
        Task<ApiResponse<ArtistProfileResponse>> CreateMyProfileAsync(int accountId, ArtistProfileRequest request);
        Task<ApiResponse<IEnumerable<ArtistProfileResponse>>> GetAllProfilesAsync();
        Task<ApiResponse<bool>> DeleteProfileAsync(int id);
    }
}
