//using ArtisanHubs.Bussiness.Services;
//using ArtisanHubs.Bussiness.Services.Carts.Implements;
//using ArtisanHubs.Data.Entities;
//using Microsoft.AspNetCore.Mvc;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using ArtisanHubs.Bussiness.Services.Carts.Interfaces;

//namespace ArtisanHubs.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class OrderDetailController : ControllerBase
//    {
//        private readonly OrderDetailService _orderDetailService;
//        private readonly ICartService _cartService;

//        public OrderDetailController(OrderDetailService orderDetailService, ICartService cartService)
//        {
//            _orderDetailService = orderDetailService;
//            _cartService = cartService;
//        }

//        [HttpPost("create-from-cart")]
//        public async Task<IActionResult> CreateOrderDetailsFromCart(
//            [FromBody] CreateOrderDetailFromCartRequest request)
//        {
//            var cart = await _cartService.GetCartByIdAsync(request.CartId);
//            if (cart == null)
//                return BadRequest("Cart not found.");

//            var result = await _orderDetailService.CreateOrderDetailsFromCartIdAsync(
//                request.CartId, // Pass the cart ID instead of the cart object
//                request.OrderId,
//                request.PickProvince,
//                request.PickDistrict,
//                request.Province,
//                request.District,
//                request.Address);

//            return Ok(result);
//        }
//    }

//    public class CreateOrderDetailFromCartRequest
//    {
//        public int CartId { get; set; }
//        public int OrderId { get; set; }
//        public string PickProvince { get; set; } = string.Empty;
//        public string PickDistrict { get; set; } = string.Empty;
//        public string Province { get; set; } = string.Empty;
//        public string District { get; set; } = string.Empty;
//        public string Address { get; set; } = string.Empty;
//    }
//}