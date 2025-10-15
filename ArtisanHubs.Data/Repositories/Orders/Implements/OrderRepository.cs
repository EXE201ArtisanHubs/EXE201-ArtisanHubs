using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Orders.Interfaces;

namespace ArtisanHubs.Data.Repositories.Orders.Implements
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ArtisanHubsDbContext _context;

        public OrderRepository(ArtisanHubsDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            return await _context.Orders.FindAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task CreateAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveAsync(Order order)
        {
            _context.Orders.Remove(order);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}