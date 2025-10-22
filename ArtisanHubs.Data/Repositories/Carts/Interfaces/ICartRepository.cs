﻿using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Repositories.Carts.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetCartByAccountIdAsync(int accountId);

        Task<Cart?> GetCartByIdAsync(int cartId); // Add this line
        // Tạo giỏ hàng mới
        Task CreateCartAsync(Cart cart);

        // Cập nhật giỏ hàng
        Task UpdateCartAsync(Cart cart);
    }
}
