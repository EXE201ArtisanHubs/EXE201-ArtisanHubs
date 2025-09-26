using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Repositories.ArtistProfiles.Implements
{
    public class ArtistProfileRepository : GenericRepository<Artistprofile>, IArtistProfileRepository
    {
        private readonly ArtisanHubsDbContext _context;

        public ArtistProfileRepository(ArtisanHubsDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Artistprofile>> GetAllAsync()
        {
            return await _context.Artistprofiles.ToListAsync();
        }

        public async Task<Artistprofile?> GetProfileByAccountIdAsync(int accountId)
        {
            return await _context.Artistprofiles
                                 .FirstOrDefaultAsync(p => p.AccountId == accountId);
        }
    }
}
