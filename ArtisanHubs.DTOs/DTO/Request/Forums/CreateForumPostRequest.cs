using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Request.Forums
{
    public class CreateForumPostRequest
    {
        [Required]
        public string Content { get; set; }

        [Required]
        public int ForumThreadId { get; set; }
    }
}
