using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;

namespace ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces
{
    public interface IArtistProfileRepository : IGenericRepository<Artistprofile>
    {
        Task<IEnumerable<Artistprofile>> GetAllAsync();
        Task<Artistprofile?> GetProfileWithProductsAsync(int artistID);
        Task<Artistprofile?> GetProfileByAccountIdAsync(int id);
        Task<IPaginate<Artistprofile>> GetPagedAsync(
        Expression<Func<Artistprofile, bool>>? predicate,
        int page,
        int size,
        string? searchTerm = null
    );
        
        //Task<IEnumerable<Artistprofile>> GetAllArtistsAsync();
    }
}
