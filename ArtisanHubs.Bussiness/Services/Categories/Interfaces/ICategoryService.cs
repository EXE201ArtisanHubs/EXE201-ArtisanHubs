using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.DTOs.DTO.Reponse.Categories;
using ArtisanHubs.DTOs.DTO.Request.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Categories.Interfaces
{
    public interface ICategoryService
    {
        Task<ApiResponse<IEnumerable<CategoryResponse>>> GetAllCategoriesAsync();
        Task<ApiResponse<CategoryResponse?>> GetCategoryByIdAsync(int categoryId);
        Task<ApiResponse<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request);
        Task<ApiResponse<CategoryResponse?>> UpdateCategoryAsync(int categoryId, UpdateCategoryRequest request);
        Task<ApiResponse<bool>> DeleteCategoryAsync(int categoryId);
    }
}
