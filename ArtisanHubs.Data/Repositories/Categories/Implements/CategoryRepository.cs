
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.Data.Repositories.Categories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtisanHubs.Data.Repositories.Categories.Implements
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly ArtisanHubsDbContext _context;
        public CategoryRepository(ArtisanHubsDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetByConditionAsync(Expression<Func<Category, bool>> condition)
        {
            return await _context.Categories.Where(condition).ToListAsync();
        }

        public async Task<bool> ExistAsync(Expression<Func<Category, bool>> condition)
        {
            return await _context.Categories.AnyAsync(condition);
        }

        public async Task<IPaginate<Category>> GetPagedAsync(
        Expression<Func<Category, bool>>? predicate,
        int page,
        int size,
        string? searchTerm = null
)
        {
            IQueryable<Category> query = _context.Set<Category>();

            // Nếu có điều kiện predicate thì apply
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                string keyword = searchTerm.ToLower();
                query = query.Where(a =>
                    a.Description.ToLower().Contains(keyword)
                );
            }

            // Phân trang
            return await query.AsNoTracking()
                              .OrderBy(a => a.CategoryId)
                              .ToPaginateAsync(page, size);
        }
    }
}
