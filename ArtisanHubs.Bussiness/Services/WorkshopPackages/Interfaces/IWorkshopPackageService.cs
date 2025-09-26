using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.DTOs.DTO.Reponse.WorkshopPackages;
using ArtisanHubs.DTOs.DTO.Request.WorkshopPackages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.WorkshopPackages.Interfaces
{
    public interface IWorkshopPackageService
    {
        Task<ApiResponse<IEnumerable<WorkshopPackageResponse>>> GetAllPackagesAsync();
        Task<ApiResponse<WorkshopPackageResponse>> GetPackageByIdAsync(int packageId);
        Task<ApiResponse<WorkshopPackageResponse>> CreatePackageAsync(WorkshopPackageRequest request);
        Task<ApiResponse<WorkshopPackageResponse>> UpdatePackageAsync(int packageId, WorkshopPackageRequest request);
        Task<ApiResponse<bool>> DeletePackageAsync(int packageId);
    }
}
