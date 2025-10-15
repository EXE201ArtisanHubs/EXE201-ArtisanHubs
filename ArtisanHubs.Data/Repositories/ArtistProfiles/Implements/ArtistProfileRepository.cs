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
            return await _context.Artistprofiles
             .Include(p => p.Achievements) 
             .ToListAsync();
        }

        public async Task<Artistprofile?> GetProfileByAccountIdAsync(int accountId)
        {
            return await _context.Artistprofiles
                                 .Include (p=>p.Achievements)
                                 .FirstOrDefaultAsync(p => p.AccountId == accountId);
        }

        //public async Task<IEnumerable<Artistprofile>> GetAllArtistsAsync()
        //{
        //    return await _context.Artistprofiles
        //                         .Include(a => a.Account) // nếu muốn lấy thông tin account (username, email…)
        //                         .ToListAsync();
        //}
    }
}
