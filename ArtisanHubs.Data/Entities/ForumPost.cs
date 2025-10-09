using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Entities
{
    public class ForumPost
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int AuthorId { get; set; }
        public virtual Account Author { get; set; } = null!;
        public int ForumThreadId { get; set; }
        public virtual ForumThread ForumThread { get; set; } = null!;
    }
}
