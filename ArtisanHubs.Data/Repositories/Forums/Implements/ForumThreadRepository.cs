using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Forums.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Repositories.Forums.Implements
{
    public class ForumThreadRepository : GenericRepository<ForumThread>, IForumThreadRepository
    {

        private readonly ArtisanHubsDbContext _context;
        public ForumThreadRepository(ArtisanHubsDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ForumThread>> GetThreadsByTopicAsync(int topicId)
        {
           return await _context.ForumThreads
                .Where(t => t.ForumTopicId == topicId)
                .Include(t => t.Author) // Lấy thông tin người tạo thread
                .ToListAsync();
        }

        public async Task<ForumThread?> GetThreadWithDetailsAsync(int threadId)
        {
            return await _context.ForumThreads
                .Include(t => t.Author)
                .Include(t => t.Posts)
                    .ThenInclude(p => p.Author)
                .FirstOrDefaultAsync(t => t.Id == threadId);
        }

        public async Task<bool> HasThreadsInTopicAsync(int topicId)
        {
            return await _context.ForumThreads.AnyAsync(thread => thread.ForumTopicId == topicId);
        }
    }
}
