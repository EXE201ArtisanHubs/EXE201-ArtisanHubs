using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Request.Accounts
{
    public class ResetPasswordWithOtpRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "OTP code is required.")]
        public string OtpCode { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirmation password is required.")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
