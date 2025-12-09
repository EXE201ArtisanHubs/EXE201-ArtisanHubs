using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ArtisanHubs.Bussiness.Services;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.DTOs.DTO.Request.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtisanHubs.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, status);
            if (!result)
                return NotFound();
            return NoContent();
        }


        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var result = await _orderService.CheckoutAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result.Message);
            return Ok(result.Data);
        }

        [HttpPost("payos-webhook")]
        public async Task<IActionResult> PayOSWebhook([FromBody] PayOSCallbackRequest request)
        {
            var result = await _orderService.UpdateOrderStatusAfterPaymentAsync(request.OrderCode, request.Status);
            if (!result)
                return BadRequest("Update failed");
            return Ok("Order status updated");
        }

        [HttpPost("return")]
        public async Task<IActionResult> SubmitOrderReturn([FromBody] OrderReturnRequest request, int accountId)
        {
            var result = await _orderService.SubmitOrderReturnAsync(request, accountId);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("returns")]
        public async Task<IActionResult> GetOrderReturns(int accountId)
        {
            var returns = await _orderService.GetOrderReturnsByUserIdAsync(accountId);
            return Ok(returns);
        }

        [HttpGet("order-returns/all")]
        public async Task<IActionResult> GetAllOrderReturns()
        {
            var result = await _orderService.GetAllOrderReturnsAsync();
            return Ok(result);
        }

        [HttpGet("my-order-ids")]
        [Authorize(Roles = "Customer,Artist,Admin")]
        public async Task<IActionResult> GetMyOrderIds()
        {
            var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orderIds = await _orderService.GetOrdersByAccountIdAsync(int.Parse(accountIdString));
            return Ok(orderIds);
        }

        [HttpGet("all")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null)
        {
            var result = await _orderService.GetAllOrdersAsync(page, size, searchTerm, status);
            return Ok(result);
        }

        [HttpGet("my-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null)
        {
            var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountIdString) || !int.TryParse(accountIdString, out int accountId))
            {
                return Unauthorized("Invalid account ID");
            }

            var result = await _orderService.GetMyOrdersAsync(accountId, page, size, searchTerm, status);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("my-orders/{orderId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrderDetail(int orderId)
        {
            var accountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountIdString) || !int.TryParse(accountIdString, out int accountId))
            {
                return Unauthorized("Invalid account ID");
            }

            var result = await _orderService.GetMyOrderDetailAsync(accountId, orderId);
            if (!result.IsSuccess)
            {
                if (result.StatusCode == 404)
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}