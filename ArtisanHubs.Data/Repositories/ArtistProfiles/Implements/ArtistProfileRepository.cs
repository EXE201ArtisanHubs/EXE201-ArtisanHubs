using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task<IPaginate<Artistprofile>> GetPagedAsync(
        Expression<Func<Artistprofile, bool>>? predicate,
        int page,
        int size,
        string? searchTerm = null)
        {
            IQueryable<Artistprofile> query = _context.Artistprofiles
                .Include(p => p.Achievements);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.ArtistName.Contains(searchTerm));
            }

            return await query.AsNoTracking()
                              .OrderBy(a => a.AccountId)
                              .ToPaginateAsync(page, size);
        }

        public async Task<Artistprofile?> GetProfileByAccountIdAsync(int accountId)
        {
            return await _context.Artistprofiles
                                 .Include (p=>p.Achievements)
                                 .FirstOrDefaultAsync(p => p.AccountId == accountId);
        }

        public async Task<Artistprofile?> GetProfileWithProductsAsync(int artistId)
        {
            return await _context.Artistprofiles
                .Include(a => a.Products) // <-- Include danh sách sản phẩm
                .Include(a => a.Achievements) // <-- Include luôn cả thành tích
                .FirstOrDefaultAsync(a => a.ArtistId == artistId);
        }
    }
}
