using System.Collections.Generic;
using System.Threading.Tasks;
using ArtisanHubs.Data.Entities;

namespace ArtisanHubs.Data.Repositories.Orders.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetAllAsync();
        Task CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task<bool> RemoveAsync(Order order);
    }
}