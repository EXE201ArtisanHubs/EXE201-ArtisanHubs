using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Request.Categories
{
    public class CreateCategoryRequest
    {
        public int? ParentId { get; set; }

        [Required]
        public string Description { get; set; }
        public string Status { get; set; } = "Active";
    }
}
