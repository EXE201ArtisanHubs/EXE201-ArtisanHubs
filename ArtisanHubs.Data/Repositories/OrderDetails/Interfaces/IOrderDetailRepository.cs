using System.Collections.Generic;
using System.Threading.Tasks;
using ArtisanHubs.Data.Entities;

namespace ArtisanHubs.Data.Repositories.OrderDetails.Interfaces
{
    public interface IOrderDetailRepository
    {
        Task<Orderdetail?> GetByIdAsync(int orderDetailId);
        Task<IEnumerable<Orderdetail>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Orderdetail>> GetAllAsync();
        Task CreateAsync(Orderdetail orderDetail);
        Task UpdateAsync(Orderdetail orderDetail);
        Task<bool> RemoveAsync(Orderdetail orderDetail);
    }
}