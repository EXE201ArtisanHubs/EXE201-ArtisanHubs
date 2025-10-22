using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Carts.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Repositories.Carts.Implements
{
    public class CartRepository : GenericRepository<Cart>,ICartRepository
    {
        private readonly ArtisanHubsDbContext _context;
        public CartRepository(ArtisanHubsDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Cart?> GetCartByIdAsync(int cartId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId);
        }
        public async Task CreateCartAsync(Cart cart)
        {
            await CreateAsync(cart);
        }

        public async Task<Cart?> GetCartByAccountIdAsync(int accountId)
        {
            return await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(i => i.Product) // Lấy cả thông tin Product
            .FirstOrDefaultAsync(c => c.AccountId == accountId);
        }

        public async Task UpdateCartAsync(Cart cart)
        {
            await UpdateAsync(cart);
        }
    }
}
