using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;
using ArtisanHubs.Data.Repositories.Accounts.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace ArtisanHubs.Data.Repositories.Accounts.Implements
{
    public class AccountRepository : GenericRepository<Account>,IAccountRepository
    {
        private readonly ArtisanHubsDbContext _context;

        public AccountRepository(ArtisanHubsDbContext context) : base(context) // ✅ truyền cho GenericRepository
        {
            _context = context;
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _context.Accounts.ToListAsync();
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _context.Accounts.FindAsync(id);
        }

        public async Task<Account?> GetByEmailAsync(string email)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(account => account.Email.ToLower() == email.ToLower());
        }
        public async Task<IPaginate<Account>> GetPagedAsync(
        Expression<Func<Account, bool>>? predicate,
        int page,
        int size,
        string? searchTerm = null
)
        {
            IQueryable<Account> query = _context.Set<Account>();

            // Nếu có điều kiện predicate thì apply
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                string keyword = searchTerm.ToLower();
                query = query.Where(a =>
                    a.Email.ToLower().Contains(keyword) ||
                    a.Username.ToLower().Contains(keyword) ||
                    a.Role.ToLower().Contains(keyword)
                );
            }

            // Phân trang
            return await query.AsNoTracking()
                              .OrderBy(a => a.AccountId)
                              .ToPaginateAsync(page, size);
        }

    }
}
