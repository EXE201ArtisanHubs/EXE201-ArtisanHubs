using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.OrderDetails.Interfaces;

namespace ArtisanHubs.Bussiness.Services
{
    public class OrderDetailService
    {
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly GHTKService _ghtkService;

        public OrderDetailService(IOrderDetailRepository orderDetailRepository, GHTKService ghtkService)
        {
            _orderDetailRepository = orderDetailRepository;
            _ghtkService = ghtkService;
        }

        public async Task<List<Orderdetail>> CreateOrderDetailsFromCartAsync(
            Cart cart,
            int orderId,
            string pickProvince,
            string pickDistrict,
            string province,
            string district,
            string address)
        {
            var orderDetails = new List<Orderdetail>();
            decimal totalShippingFee = 0;

            foreach (var cartItem in cart.CartItems)
            {
                var product = cartItem.Product;
                var unitPrice = product.DiscountPrice ?? product.Price;
                var totalPrice = unitPrice * cartItem.Quantity;

                var ghtkFeeResponse = await _ghtkService.GetShippingFeeAsync(
                    pickProvince, pickDistrict, province, district, address,
                    weight: (int)product.Weight,
                    value: (int)totalPrice
                );

                decimal shippingFee = (decimal)(ghtkFeeResponse?.Fee?.ShipFeeOnly ?? 0);
                totalShippingFee += shippingFee;

                var orderDetail = new Orderdetail
                {
                    OrderId = orderId,
                    ProductId = product.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    Product = product
                };
                await _orderDetailRepository.CreateAsync(orderDetail);
                orderDetails.Add(orderDetail);
            }
            return orderDetails;
        }
    }
}
