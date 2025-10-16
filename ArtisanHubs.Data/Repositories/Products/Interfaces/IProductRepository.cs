using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Paginate;

namespace ArtisanHubs.Data.Repositories.Products.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByArtistIdAsync(int artistId);
        Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId);
        Task<bool> ProductExistsByNameAsync(int artistId, string productName);
        Task<Product?> GetProductWithDetailsAsync(int productId);
        Task<IPaginate<Product>> GetPagedAsync(
        Expression<Func<Product, bool>>? predicate,
        int page,
        int size,
        string? searchTerm = null);
        Task<IPaginate<Product>> GetFilteredProductsAsync(
        Expression<Func<Product, bool>>? predicate,
        Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy,
        int page,
        int size);
    }
}
