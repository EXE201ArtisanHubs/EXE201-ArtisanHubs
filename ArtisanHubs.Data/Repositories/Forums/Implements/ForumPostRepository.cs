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
    public class ForumPostRepository : GenericRepository<ForumPost>, IForumPostRepository
    {
        private readonly ArtisanHubsDbContext _context;
        public ForumPostRepository(ArtisanHubsDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> CheckIfThreadExistsAsync(int threadId)
        {
            return await _context.ForumPosts.AnyAsync(t => t.Id == threadId);
        }

        public async Task<ForumPost?> GetPostWithAuthorAsync(int postId)
        {
            return await _context.ForumPosts
                .Include(post => post.Author)
                .FirstOrDefaultAsync(post => post.Id == postId);
        }
    }
}
