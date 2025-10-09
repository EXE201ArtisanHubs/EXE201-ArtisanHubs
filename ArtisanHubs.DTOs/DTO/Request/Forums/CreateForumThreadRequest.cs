using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Request.Forums
{
    public class CreateForumThreadRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        public string InitialPostContent { get; set; }
        [Required]
        public int ForumTopicId { get; set; }
    }
}
