using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Reponse.Forums
{
    public class AuthorResponse
    {
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string? Avatar { get; set; }
    }
}
