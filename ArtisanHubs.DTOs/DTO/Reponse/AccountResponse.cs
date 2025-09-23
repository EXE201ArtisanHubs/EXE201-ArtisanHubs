using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTOs.Reponse
{
    public class AccountResponse
    {
        public int AccountId { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public string? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime? CreatedAt { get; set; }
    }
}
