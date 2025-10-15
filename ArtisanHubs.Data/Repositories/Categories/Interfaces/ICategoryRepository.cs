using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;

namespace ArtisanHubs.Data.Repositories.Categories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    { 
        Task<IEnumerable<Category>> GetByConditionAsync(Expression<Func<Category, bool>> condition);
        Task<bool> ExistAsync(Expression<Func<Category, bool>> condition);
        Task<IPaginate<Category>> GetPagedAsync(
        Expression<Func<Category, bool>>? predicate,
        int page,
        int size,
        string? searchTerm = null);
    }
}
