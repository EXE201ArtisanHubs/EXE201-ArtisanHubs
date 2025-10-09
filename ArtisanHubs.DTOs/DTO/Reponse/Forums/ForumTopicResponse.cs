using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Reponse.Forums
{
    public class ForumTopicResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
    }
}
