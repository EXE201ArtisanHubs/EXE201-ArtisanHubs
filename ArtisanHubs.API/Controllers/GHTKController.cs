using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class GHTKController : ControllerBase
{
    private readonly GHTKService _ghtkService;

    public GHTKController(GHTKService ghtkService)
    {
        _ghtkService = ghtkService;
    }

    [HttpGet("calculate-fee")]
    public async Task<IActionResult> CalculateFee(
        [FromQuery] string pickProvince,
        [FromQuery] string pickDistrict,
        [FromQuery] string province,
        [FromQuery] string district,
        [FromQuery] string address,
        [FromQuery] int weight,
        [FromQuery] int value)
    {
        var result = await _ghtkService.GetShippingFeeAsync(
            pickProvince, pickDistrict, province, district, address, weight, value);

        if (result == null)
            return BadRequest("Không thể lấy thông tin từ GHTK.");

        if (!result.Success || !result.Fee.Delivery)
            return BadRequest(result.Message ?? "Địa chỉ không giao được.");

        // Trả gọn thông tin phí ship
        return Ok(new
        {
            success = true,
            shipFee = result.Fee.ShipFeeOnly,
            displayText = result.Fee.Options?.ShipMoneyText
        });
    }
}

