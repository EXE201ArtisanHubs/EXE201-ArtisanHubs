using ArtisanHubs.Bussiness.Services.Payment;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.OrderDetails.Interfaces;
using ArtisanHubs.Data.Repositories.Orders.Interfaces;
using ArtisanHubs.DTOs.DTO.Request.Payment;
using CloudinaryDotNet.Actions;
using Net.payOS.Types;

public class OrderPaymentService
{
    private readonly PayOSService _payOSService;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly ArtisanHubsDbContext _context;

    public OrderPaymentService(
        PayOSService payOSService,
        IOrderRepository orderRepository,
        IOrderDetailRepository orderDetailRepository,
        ArtisanHubsDbContext context)
    {
        _payOSService = payOSService;
        _orderRepository = orderRepository;
        _orderDetailRepository = orderDetailRepository;
        _context = context;

    }

    public async Task<string> ProcessPaymentAndCreateOrderAsync(
    PaymentRequest req,
    int accountId,
    List<Orderdetail> orderDetails,
    string shippingAddress)
    {
        // 1. Create a new order instance
        var order = new Order
        {
            AccountId = accountId,
            OrderDate = DateTime.UtcNow,
            ShippingAddress = shippingAddress,
            TotalAmount = req.amount,
            Status = "Pending",
            PaymentMethod = "PayOS",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 👉 Save order first time only
        await _orderRepository.CreateAsync(order);

        // 2. Create payment link
        var result = await _payOSService.CreatePaymentLinkAsync(
            req.OrderId,
            req.amount,
            req.description,
            req.returnUrl,
            req.cancelUrl
        );
        return result.checkoutUrl;
    }
}