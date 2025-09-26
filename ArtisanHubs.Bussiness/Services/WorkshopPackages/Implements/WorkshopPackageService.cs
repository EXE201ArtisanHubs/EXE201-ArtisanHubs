using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.WorkshopPackages.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.WorkshopPackages.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.WorkshopPackages;
using ArtisanHubs.DTOs.DTO.Request.WorkshopPackages;
using ArtisanHubs.DTOs.DTOs.Reponse;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.WorkshopPackages.Implements
{
    public class WorkshopPackageService : IWorkshopPackageService
    {
        private readonly IWorkshopPackageRepository _packageRepository;
        private readonly IMapper _mapper;

        public WorkshopPackageService(IWorkshopPackageRepository packageRepository, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        // GET ALL
        public async Task<ApiResponse<IEnumerable<WorkshopPackageResponse>>> GetAllPackagesAsync()
        {
            try
            {
                var packages = await _packageRepository.GetAllAsync();
                var packageResponses = _mapper.Map<IEnumerable<WorkshopPackageResponse>>(packages);
                return ApiResponse<IEnumerable<WorkshopPackageResponse>>.SuccessResponse(packageResponses, "Packages retrieved successfully.");
            }
            catch (Exception ex)
            {
                // TODO: Ghi log lỗi (ex) vào hệ thống logging của bạn
                return ApiResponse<IEnumerable<WorkshopPackageResponse>>.FailResponse("An unexpected error occurred while retrieving packages.", 500);
            }
        }

        // GET BY ID
        public async Task<ApiResponse<WorkshopPackageResponse>> GetPackageByIdAsync(int packageId)
        {
            try
            {
                var package = await _packageRepository.GetByIdAsync(packageId);

                if (package == null)
                {
                    return ApiResponse<WorkshopPackageResponse>.FailResponse("Package not found.", 404);
                }

                var packageResponse = _mapper.Map<WorkshopPackageResponse>(package);
                return ApiResponse<WorkshopPackageResponse>.SuccessResponse(packageResponse, "Get package successful.");
            }
            catch (Exception ex)
            {
                // TODO: Ghi log lỗi (ex)
                return ApiResponse<WorkshopPackageResponse>.FailResponse("An unexpected error occurred.", 500);
            }
        }

        // CREATE
        public async Task<ApiResponse<WorkshopPackageResponse>> CreatePackageAsync(WorkshopPackageRequest request)
        {
            try
            {
                var packageEntity = _mapper.Map<Workshoppackage>(request);

                await _packageRepository.CreateAsync(packageEntity);

                var packageResponse = _mapper.Map<WorkshopPackageResponse>(packageEntity);
                return ApiResponse<WorkshopPackageResponse>.SuccessResponse(packageResponse, "Package created successfully.", 201);
            }
            catch (Exception ex)
            {
                // TODO: Ghi log lỗi (ex)
                return ApiResponse<WorkshopPackageResponse>.FailResponse("An error occurred while creating the package.", 500);
            }
        }

        // UPDATE
        public async Task<ApiResponse<WorkshopPackageResponse>> UpdatePackageAsync(int packageId, WorkshopPackageRequest request)
        {
            try
            {
                var existingPackage = await _packageRepository.GetByIdAsync(packageId);
                if (existingPackage == null)
                {
                    return ApiResponse<WorkshopPackageResponse>.FailResponse("Package to update not found.", 404);
                }

                // Bước 2: Commit thay đổi xuống database
                await _packageRepository.UpdateAsync(existingPackage);

                var updatedResponse = _mapper.Map<WorkshopPackageResponse>(existingPackage);

                return ApiResponse<WorkshopPackageResponse>.SuccessResponse(updatedResponse, "Package updated successfully.");
            }
            catch (Exception ex)
            {
                // TODO: Ghi log lỗi (ex)
                return ApiResponse<WorkshopPackageResponse>.FailResponse("An error occurred while updating the package.", 500);
            }
        }

        // DELETE
        public async Task<ApiResponse<bool>> DeletePackageAsync(int packageId)
        {
            try
            {
                var packageToDelete = await _packageRepository.GetByIdAsync(packageId);
                if (packageToDelete == null)
                {
                    return ApiResponse<bool>.FailResponse("Package to delete not found.", 404);
                }

                // Bước 2: Commit thay đổi và kiểm tra kết quả
                var result = await _packageRepository.RemoveAsync(packageToDelete);

                if (!result)
                {
                    // Lỗi khi commit xuống DB -> Lỗi server
                    return ApiResponse<bool>.FailResponse("Error saving changes to the database.", 500);
                }

                return ApiResponse<bool>.SuccessResponse(true, "Package deleted successfully.");
            }
            catch (Exception ex)
            {
                // TODO: Ghi log lỗi (ex)
                return ApiResponse<bool>.FailResponse("An unexpected error occurred during deletion.", 500);
            }
        }
    }
}