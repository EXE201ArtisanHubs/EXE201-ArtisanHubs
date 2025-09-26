using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.Categories.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Categories.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.Categories;
using ArtisanHubs.DTOs.DTO.Request.Categories;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Categories.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepo, IMapper mapper)
        {
            _categoryRepo = categoryRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<CategoryResponse>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepo.GetAllAsync();
                var response = _mapper.Map<IEnumerable<CategoryResponse>>(categories);
                return ApiResponse<IEnumerable<CategoryResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ở đây (nếu có)
                return ApiResponse<IEnumerable<CategoryResponse>>.FailResponse($"An unexpected error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<CategoryResponse?>> GetCategoryByIdAsync(int categoryId)
        {
            try
            {
                var category = await _categoryRepo.GetByIdAsync(categoryId);
                if (category == null)
                {
                    return ApiResponse<CategoryResponse?>.FailResponse("Category not found.", 404);
                }
                var response = _mapper.Map<CategoryResponse>(category);
                return ApiResponse<CategoryResponse?>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryResponse?>.FailResponse($"An unexpected error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request)
        {
            try
            {
                if (request.ParentId.HasValue)
                {
                    var parentExists = await _categoryRepo.GetByIdAsync(request.ParentId.Value);
                    if (parentExists == null)
                    {
                        return ApiResponse<CategoryResponse>.FailResponse("Parent category not found.", 400);
                    }
                }

                var categoryEntity = _mapper.Map<Category>(request);
                await _categoryRepo.CreateAsync(categoryEntity);
                var response = _mapper.Map<CategoryResponse>(categoryEntity);
                return ApiResponse<CategoryResponse>.SuccessResponse(response, "Category created successfully.", 201);
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryResponse>.FailResponse($"An unexpected error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<CategoryResponse?>> UpdateCategoryAsync(int categoryId, UpdateCategoryRequest request)
        {
            try
            {
                var existingCategory = await _categoryRepo.GetByIdAsync(categoryId);
                if (existingCategory == null)
                {
                    return ApiResponse<CategoryResponse?>.FailResponse("Category not found.", 404);
                }

                _mapper.Map(request, existingCategory);
                await _categoryRepo.UpdateAsync(existingCategory);
                var response = _mapper.Map<CategoryResponse>(existingCategory);
                return ApiResponse<CategoryResponse?>.SuccessResponse(response, "Category updated successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryResponse?>.FailResponse($"An unexpected error occurred: {ex.Message}", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(int categoryId)
        {
            try
            {
                var categoryToDelete = await _categoryRepo.GetByIdAsync(categoryId);
                if (categoryToDelete == null)
                {
                    return ApiResponse<bool>.FailResponse("Category not found.", 404);
                }

                await _categoryRepo.RemoveAsync(categoryToDelete);
                return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully.");
            }
            catch (Exception ex)
            {
                // Ví dụ: Lỗi khóa ngoại khi xóa danh mục đã có sản phẩm
                return ApiResponse<bool>.FailResponse($"An unexpected error occurred: {ex.Message}", 500);
            }
        }
    }
}
