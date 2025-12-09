using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace ArtisanHubs.DTOs.DTO.Request.Accounts
{
    public class UpdateAccountRequest
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = null!;

        [Phone]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public string? Gender { get; set; }

        public DateOnly? Dob { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}
