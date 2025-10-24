using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtisanHubs.API.DTOs.Common;
using ArtisanHubs.Bussiness.Services.Carts.Interfaces;
using ArtisanHubs.Bussiness.Services.Payment;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Orders.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SendGrid.Helpers.Mail;

namespace ArtisanHubs.Bussiness.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ArtisanHubsDbContext _dbContext;
        private readonly PayOSService PayOSService;
        private readonly ICartService _cartService;
        private readonly GHTKService _gHTKService;
        private readonly AdminService _adminSerivce;

        public OrderService(IOrderRepository orderRepository, ArtisanHubsDbContext dbContext, PayOSService payOSService, ICartService cartService, GHTKService gHTKService, AdminService adminSerivce)
        {
            _orderRepository = orderRepository;
            _dbContext = dbContext;
            PayOSService = payOSService;
            _cartService = cartService;
            _gHTKService = gHTKService;
            _adminSerivce = adminSerivce;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var allowedStatuses = new[] { "Doing", "In transit", "Done" };
            if (!allowedStatuses.Contains(newStatus))
                return false;

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return false;

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
            return true;
        }

        public async Task<ApiResponse<object>> CheckoutAsync(CheckoutRequest request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var cartItems = await _dbContext.CartItems
            .Include(ci => ci.Product)
            .Include(ci => ci.Cart)
            .Where(ci => request.CartItemIds.Contains(ci.Id) && ci.Cart.AccountId == request.AccountId)
            .ToListAsync();

                if (cartItems == null || !cartItems.Any())
                    return ApiResponse<object>.FailResponse("No valid cart items found.", 404);

                // 2. Tính tổng giá trị và tổng khối lượng đơn hàng
                decimal subtotal = 0;
                int totalWeight = 0;
                foreach (var item in cartItems)
                {
                    var product = item.Product;
                    if (product == null)
                        throw new Exception($"Product {item.ProductId} not found");

                    var unitPrice = product.DiscountPrice ?? product.Price;
                    subtotal += unitPrice * item.Quantity;
                    totalWeight += ((int?)product.Weight ?? 0) * item.Quantity;
                }

                // 3. Gọi GHTKService để lấy phí ship thực tế
                // Lưu ý: pickProvince/pickDistrict nên lấy từ config hoặc thông tin shop
                var pickProvince = "Hà Nội";
                var pickDistrict = "Quận Hai Bà Trưng";
                var province = request.ShippingAddress.Province;
                var district = request.ShippingAddress.District;
                var address = request.ShippingAddress.Street;
                int value = (int)subtotal;

                var ghtkFee = await _gHTKService.GetShippingFeeAsync(
                    pickProvince, pickDistrict, province, district, address, totalWeight, value
                );

                if (ghtkFee == null || ghtkFee.Fee == null)
                    return ApiResponse<object>.FailResponse("Không lấy được phí vận chuyển từ GHTK.", 500);

                decimal shippingFee = (decimal)ghtkFee.Fee.Fee;
                decimal totalAmount = subtotal + shippingFee;

                // 4. Tạo Order
                var order = new Order
                {
                    AccountId = request.AccountId,
                    ShippingAddress = $"{request.ShippingAddress.Street}, {request.ShippingAddress.Ward}, {request.ShippingAddress.District}, {request.ShippingAddress.Province}",
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    ShippingFee = shippingFee,
                    PaymentMethod = "PayOS"
                };
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();
                long orderCode = long.Parse($"{order.OrderId}{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
                order.OrderCode = orderCode;
                await _dbContext.SaveChangesAsync();

                // 5. Thêm OrderDetails
                foreach (var item in cartItems)
                {
                    var product = item.Product!;
                    var unitPrice = product.DiscountPrice ?? product.Price;
                    var totalPrice = unitPrice * item.Quantity;

                    var orderDetail = new Orderdetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = totalPrice
                    };
                    _dbContext.Orderdetails.Add(orderDetail);
                }
                await _dbContext.SaveChangesAsync();

                // 6. Commit transaction trước khi gọi API ngoài
                await transaction.CommitAsync();

                // 7. Gọi PayOS để lấy link thanh toán

                var paymentResult = await PayOSService.CreatePaymentLinkAsync(
                orderCode.ToString(),
                totalAmount,
                "Order Payment",
                "https://example.com/success",
                "https://example.com/cancel"
                );


                if (paymentResult != null && !string.IsNullOrEmpty(paymentResult.checkoutUrl))
                {
                    order.Status = "Waiting for payment";
                }
                else
                {
                    order.Status = "Payment failed";
                }

                await _dbContext.SaveChangesAsync();

                return ApiResponse<object>.SuccessResponse(new
                {
                    Order = order,
                    PaymentUrl = paymentResult?.checkoutUrl
                }, "Checkout successful");
            }
            catch (Exception ex)
            {
                if (transaction.GetDbTransaction().Connection != null)
                {
                    await transaction.RollbackAsync();
                }

                return ApiResponse<object>.FailResponse($"Checkout failed: {ex.Message}", 500);
            }
        }

        public async Task<bool> UpdateOrderStatusAfterPaymentAsync(long orderCode, string paymentStatus)
        {
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null) return false;

            if (paymentStatus == "PAID")
            {
                order.Status = "Paid";
                order.UpdatedAt = DateTime.UtcNow;

                // 🔥 Gọi tạo hoa hồng cho đơn hàng đã thanh toán
                decimal platformRate = 0.10m; // 10% hoa hồng sàn
                await _adminSerivce.CreateCommissionForPaidOrderAsync(order.OrderId, platformRate);
            }
            else if (paymentStatus == "CANCELLED")
            {
                order.Status = "Cancelled";
                order.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                order.Status = "Payment failed";
                order.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _dbContext.Orders
                .Where(o => o.AccountId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<ApiResponse<object>> SubmitOrderReturnAsync(OrderReturnRequest request, int userId)
        {
            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == request.OrderId && o.AccountId == userId);
            if (order == null)
                return ApiResponse<object>.FailResponse("Order not found or access denied.");

            var orderReturn = new OrderReturn
            {
                OrderId = request.OrderId,
                Reason = request.Reason,
                BankAccountName = request.BankAccountName,
                BankAccountNumber = request.BankAccountNumber,
                BankName = request.BankName,
                RequestedAt = DateTime.UtcNow,
                Status = "Pending"
            };
            _dbContext.OrderReturns.Add(orderReturn);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<object>.SuccessResponse("Return request submitted successfully.");
        }

        public async Task<IEnumerable<OrderReturn>> GetOrderReturnsByUserIdAsync(int userId)
        {
            // Lấy các đơn hàng của user
            var orderIds = await _dbContext.Orders
                .Where(o => o.AccountId == userId)
                .Select(o => o.OrderId)
                .ToListAsync();

            // Lấy các phiếu hoàn trả liên quan đến các đơn hàng đó
            return await _dbContext.OrderReturns
                .Where(or => orderIds.Contains(or.OrderId))
                .OrderByDescending(or => or.RequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderReturn>> GetAllOrderReturnsAsync()
        {
            return await _dbContext.OrderReturns
                .OrderByDescending(or => or.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByAccountIdAsync(int accountId)
        {
            return await _dbContext.Orders
                .Where(o => o.AccountId == accountId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}
