using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.DTOs.DTO.Reponse.Categories;
using ArtisanHubs.DTOs.DTO.Request.Categories;

namespace ArtisanHubs.Bussiness.Services.Categories.Interfaces
{
    public interface ICategoryService
    {
        Task<ApiResponse<IPaginate<Category>>> GetAllCategoryAsync(int page, int size, string? searchTerm = null);
        Task<ApiResponse<CategoryResponse?>> GetCategoryByIdAsync(int categoryId);
        Task<ApiResponse<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request);
        Task<ApiResponse<CategoryResponse?>> UpdateCategoryAsync(int categoryId, UpdateCategoryRequest request);
        Task<ApiResponse<bool>> DeleteCategoryAsync(int categoryId);
        Task<ApiResponse<IEnumerable<CategoryResponse>>> GetParentCategoriesAsync();
        Task<ApiResponse<IEnumerable<CategoryResponse>>> GetChildCategoriesAsync(int parentId);
    }
}
