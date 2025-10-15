using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ArtisanHubs.Bussiness.Services.RefreshTokens;
using ArtisanHubs.Bussiness.Services.Accounts.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class RefreshTokenController : ControllerBase
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAccountService _accountService;

    public RefreshTokenController(IRefreshTokenService refreshTokenService, IAccountService accountService)
    {
        _refreshTokenService = refreshTokenService;
        _accountService = accountService;
    }

    // Tạo refresh token mới (ví dụ: khi đăng nhập thành công)
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromQuery] int accountId)
    {
        var refreshToken = await _refreshTokenService.GenerateAndStoreRefreshToken(accountId);
        return Ok(new { refreshToken });
    }

    // Làm mới access token bằng refresh token
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        try
        {
            var accessToken = await _refreshTokenService.RefreshAccessToken(refreshToken);
            return Ok(new { accessToken });
        }
        catch
        {
            return Unauthorized();
        }
    }

    // Xóa refresh token (logout)
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] string refreshToken)
    {
        var result = await _refreshTokenService.DeleteRefreshToken(refreshToken);
        if (result)
            return Ok();
        return NotFound();
    }
}