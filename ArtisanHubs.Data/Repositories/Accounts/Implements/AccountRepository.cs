using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Accounts.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
