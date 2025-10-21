using ArtisanHubs.Bussiness.Services;
using ArtisanHubs.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ArtisanHubs.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderDetailController : ControllerBase
    {
        private readonly OrderDetailService _orderDetailService;

        public OrderDetailController(OrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

        [HttpPost("create-from-cart")]
        public async Task<IActionResult> CreateOrderDetailsFromCart(
            [FromBody] CreateOrderDetailFromCartRequest request)
        {
            if (request.Cart == null)
                return BadRequest("Cart is required.");

            var result = await _orderDetailService.CreateOrderDetailsFromCartAsync(
                request.Cart,
                request.OrderId,
                request.PickProvince,
                request.PickDistrict,
                request.Province,
                request.District,
                request.Address);

            return Ok(result);
        }
    }

    public class CreateOrderDetailFromCartRequest
    {
        public Cart Cart { get; set; } = null!;
        public int OrderId { get; set; }
        public string PickProvince { get; set; } = string.Empty;
        public string PickDistrict { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}