using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Products.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Repositories.Products.Implements
{
    public class FavoriteProductRepository : GenericRepository<FavoriteProduct>, IFavoriteProductRepository
    {
        private readonly ArtisanHubsDbContext _context;
        public FavoriteProductRepository(ArtisanHubsDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<FavoriteProduct?> GetFavoriteAsync(int accountId, int productId)
        {
            return await _context.FavoriteProducts.FirstOrDefaultAsync(fp => fp.AccountId == accountId && fp.ProductId == productId);
        }
    }
}
