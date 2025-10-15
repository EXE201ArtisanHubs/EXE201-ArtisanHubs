using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ArtisanHubs.DTOs.DTOs.Request.Accounts
{
    public class AccountRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // Sửa lại Role để không có giá trị mặc định
        [Required]
        public string Role { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Các thuộc tính không bắt buộc vẫn giữ nguyên là nullable
        public string? Phone { get; set; }
        public string? Address { get; set; }
        //public string? Avatar { get; set; }
        public string? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }
}