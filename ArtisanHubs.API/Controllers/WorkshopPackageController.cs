using ArtisanHubs.Bussiness.Services.WorkshopPackages.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.WorkshopPackages;
using Microsoft.AspNetCore.Mvc;

namespace ArtisanHubs.API.Controllers
{
    [ApiController]
    [Route("api/v1/workshoppackages")]
    public class WorkshopPackageController : ControllerBase
    {
        private readonly IWorkshopPackageService _packageService;

        public WorkshopPackageController(IWorkshopPackageService packageService)
        {
            _packageService = packageService;
        }

        /// <summary>
        /// Lấy danh sách tất cả các gói workshop
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllPackages()
        {
            var result = await _packageService.GetAllPackagesAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy một gói workshop theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPackageById(int id)
        {
            var result = await _packageService.GetPackageByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Tạo mới một gói workshop
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePackage([FromBody] WorkshopPackageRequest request)
        {
            var result = await _packageService.CreatePackageAsync(request);

            // Đặc biệt cho POST (Create), nếu thành công (201 Created), trả về CreatedAtAction
            // để client biết URL của tài nguyên mới được tạo.
            if (result.StatusCode == 201)
            {
                return CreatedAtAction(nameof(GetPackageById), new { id = result.Data.PackageId }, result);
            }

            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Cập nhật một gói workshop
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePackage(int id, [FromBody] WorkshopPackageRequest request)
        {
            var result = await _packageService.UpdatePackageAsync(id, request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Xóa một gói workshop
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var result = await _packageService.DeletePackageAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
