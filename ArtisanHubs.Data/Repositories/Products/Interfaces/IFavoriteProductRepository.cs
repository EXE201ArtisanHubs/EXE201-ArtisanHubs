using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Repositories.Products.Interfaces
{
    public interface IFavoriteProductRepository : IGenericRepository<FavoriteProduct>
    {
        Task<FavoriteProduct?> GetFavoriteAsync(int accountId, int productId);
    }
}
