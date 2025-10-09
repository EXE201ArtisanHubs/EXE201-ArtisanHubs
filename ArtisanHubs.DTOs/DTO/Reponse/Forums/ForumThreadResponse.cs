using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.DTOs.DTO.Reponse.Forums
{
    public class ForumThreadResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public AuthorResponse Author { get; set; }
        public List<ForumPostResponse> Posts { get; set; }
    }
}
