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
    public class ForumTopicRepository : GenericRepository<ForumTopic>, IForumTopicRepository
    {
        private readonly ArtisanHubsDbContext _context = new ArtisanHubsDbContext();
        public ForumTopicRepository(ArtisanHubsDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ForumTopic>> GetAll()
        {
            return await _context.ForumTopics.ToListAsync();
        }

        public async Task<bool> ExistsByTitleAsync(string title)
        {
            return await _context.ForumTopics.AnyAsync(a => a.Title == title);
        }

    }
}
