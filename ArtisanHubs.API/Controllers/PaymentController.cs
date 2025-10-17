using ArtisanHubs.Data.Entities;
using ArtisanHubs.DTOs.DTO.Request;
using ArtisanHubs.DTOs.DTO.Request.Payment;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly OrderPaymentService _orderPaymentService;

    public PaymentsController(OrderPaymentService orderPaymentService)
    {
        _orderPaymentService = orderPaymentService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest req)
    {
        if (req == null)
            return BadRequest("Invalid request.");

        try
        {
            // Use accountId from request
            int accountId = req.AccountId;
            var orderDetails = new List<Orderdetail>(); // Populate from request
            string shippingAddress = "Sample Address"; // Populate from request

            var checkoutUrl = await _orderPaymentService.ProcessPaymentAndCreateOrderAsync(req, accountId, orderDetails, shippingAddress);
            return Ok(new { checkoutUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refund/{orderId}")]
    public async Task<IActionResult> RefundOrder(int orderId, [FromBody] string reason)
    {
        try
        {
            var message = await _orderPaymentService.RefundOrderAsync(orderId, reason);
            return Ok(new { message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}