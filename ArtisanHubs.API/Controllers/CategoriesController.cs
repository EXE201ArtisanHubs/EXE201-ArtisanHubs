using ArtisanHubs.Bussiness.Services.Categories.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtisanHubs.API.Controllers
{
    [Route("api/categories")]
    [ApiController]

    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? searchTerm = null)
        {
            var result = await _categoryService.GetAllCategoryAsync(page, size, searchTerm);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")] // <-- Endpoint này cũng công khai
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("parents")]
        public async Task<IActionResult> GetParentCategories()
        {
            var result = await _categoryService.GetParentCategoriesAsync();
            return Ok(result);
        }

        [HttpGet("{parentId}/children")]
        public async Task<IActionResult> GetChildCategories(int parentId)
        {
            var result = await _categoryService.GetChildCategoriesAsync(parentId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // <-- Chỉ Admin được tạo
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            var result = await _categoryService.CreateCategoryAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // <-- Chỉ Admin được sửa
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            var result = await _categoryService.UpdateCategoryAsync(id, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // <-- Chỉ Admin được xóa
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
