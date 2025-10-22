using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtisanHubs.Bussiness.Services.Carts.Implements;
using ArtisanHubs.Bussiness.Services.Carts.Interfaces;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.OrderDetails.Interfaces;

namespace ArtisanHubs.Bussiness.Services
{
    public class OrderDetailService
    {
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly GHTKService _ghtkService;
        private readonly ICartService _cartService;

        public OrderDetailService(IOrderDetailRepository orderDetailRepository, GHTKService ghtkService, ICartService cartService)
        {
            _orderDetailRepository = orderDetailRepository;
            _ghtkService = ghtkService;
            _cartService = cartService;
        }

        public async Task<List<Orderdetail>> CreateOrderDetailsFromCartIdAsync(
            int cartId,
            int orderId,
            string pickProvince,
            string pickDistrict,
            string province,
            string district,
            string address)
        {
            var cart = await _cartService.GetCartByIdAsync(cartId);
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                throw new Exception("Cart not found or empty.");

            var orderDetails = new List<Orderdetail>();

            foreach (var cartItem in cart.CartItems)
            {
                var product = cartItem.Product;
                var unitPrice = product.DiscountPrice ?? product.Price;
                var totalPrice = unitPrice * cartItem.Quantity;

                // 2. Calculate shipping fee for each product
                var ghtkFeeResponse = await _ghtkService.GetShippingFeeAsync(
                    pickProvince, pickDistrict, province, district, address,
                    weight: (int)product.Weight,
                    value: (int)totalPrice
                ); decimal shippingFee = (decimal)(ghtkFeeResponse?.Fee?.Fee ?? 0);


                // 3. Save order detail (add ShippingFee property if needed)
                var orderDetail = new Orderdetail
                {
                    OrderId = orderId,
                    ProductId = product.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    ShippingFee = shippingFee,
                    Product = product
                };
                await _orderDetailRepository.CreateAsync(orderDetail);
                orderDetails.Add(orderDetail);
            }
            return orderDetails;
        }
    }
}
