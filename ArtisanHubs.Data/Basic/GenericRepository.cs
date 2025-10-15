﻿using ArtisanHubs.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ArtisanHubs.Data.Basic
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ArtisanHubsDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public GenericRepository(ArtisanHubsDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<int> CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<T?> GetByConditionAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }
        public IQueryable<T> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }
    }
}
