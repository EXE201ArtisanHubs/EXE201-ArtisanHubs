using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard-statistics")]
    public async Task<IActionResult> GetDashboardStatistics()
    {
        var stats = await _adminService.GetDashboardStatisticsAsync();
        return Ok(stats);
    }

    [HttpGet("WithdrawRequests/Pending")]
    public async Task<IActionResult> GetPendingWithdrawRequests()
    {
        var result = await _adminService.GetPendingWithdrawRequestsAsync();
        return Ok(result);
    }

    [HttpGet("Commissions/Unpaid/{artistId}")]
    public async Task<IActionResult> GetUnpaidCommissions(int artistId)
    {
        var result = await _adminService.GetUnpaidCommissionsAsync(artistId);
        return Ok(result);
    }

    [HttpPost("WithdrawRequests/Approve/{withdrawRequestId}")]
    public async Task<IActionResult> ApproveWithdrawRequest(int withdrawRequestId)
    {
        var success = await _adminService.ApproveWithdrawRequestAsync(withdrawRequestId);
        if (!success)
            return BadRequest("Unable to approve withdrawal request.");
        return Ok("Withdrawal request approved.");
    }
}