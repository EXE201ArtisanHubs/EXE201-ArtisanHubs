using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Entities
{
    public class ForumThread
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to Account (who created this thread)
        public int AuthorId { get; set; }
        public virtual Account Author { get; set; } = null!;

        // Foreign key to ForumTopic (which topic this thread belongs to)
        public int ForumTopicId { get; set; }
        public virtual ForumTopic ForumTopic { get; set; } = null!;

        // Navigation property
        public virtual ICollection<ForumPost> Posts { get; set; } = new List<ForumPost>();
    }
}
