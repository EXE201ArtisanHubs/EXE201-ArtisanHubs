using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Repositories.ArtistProfiles.Interfaces
{
    public interface IArtistProfileRepository : IGenericRepository<Artistprofile>
    {
        Task<IEnumerable<Artistprofile>> GetAllAsync();
        Task<Artistprofile?> GetProfileByAccountIdAsync(int id);
    }
}
