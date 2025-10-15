using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.OrderDetails.Interfaces;

namespace ArtisanHubs.Data.Repositories.OrderDetails.Implements
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly ArtisanHubsDbContext _context;

        public OrderDetailRepository(ArtisanHubsDbContext context)
        {
            _context = context;
        }

        public async Task<Orderdetail?> GetByIdAsync(int orderDetailId)
        {
            return await _context.Orderdetails.FindAsync(orderDetailId);
        }

        public async Task<IEnumerable<Orderdetail>> GetByOrderIdAsync(int orderId)
        {
            return await _context.Orderdetails
                .Where(od => od.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Orderdetail>> GetAllAsync()
        {
            return await _context.Orderdetails.ToListAsync();
        }

        public async Task CreateAsync(Orderdetail orderDetail)
        {
            await _context.Orderdetails.AddAsync(orderDetail);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Orderdetail orderDetail)
        {
            _context.Orderdetails.Update(orderDetail);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveAsync(Orderdetail orderDetail)
        {
            _context.Orderdetails.Remove(orderDetail);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}