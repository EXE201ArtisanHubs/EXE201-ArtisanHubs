﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Basic
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        IQueryable<T> GetQueryable();
        Task<T?> GetByIdAsync(int id);
        Task<int> CreateAsync(T entity);
        Task<int> UpdateAsync(T entity);
        Task<bool> RemoveAsync(T entity);
        Task<T?> GetByConditionAsync(Expression<Func<T, bool>> predicate);
    }
}
