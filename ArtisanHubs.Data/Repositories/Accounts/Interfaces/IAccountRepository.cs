using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;

namespace ArtisanHubs.Data.Repositories.Accounts.Interfaces
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Task<IEnumerable<Account>> GetAllAsync();
        Task<Account?> GetByIdAsync(int id);
        Task<Account?> GetByEmailAsync(string email);
        Task<IPaginate<Account>> GetPagedAsync(
        Expression<Func<Account, bool>>? predicate,
        int page,
        int size,
        string? searchTerm = null
    );
    }
}
