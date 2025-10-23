using System.Collections.Generic;
using System.Threading.Tasks;
using ArtisanHubs.Bussiness.Services;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.DTOs.DTO.Request.Orders;
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
    }
}