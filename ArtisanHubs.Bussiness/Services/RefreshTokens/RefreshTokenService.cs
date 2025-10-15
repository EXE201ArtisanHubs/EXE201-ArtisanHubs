using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ArtisanHubs.Bussiness.Services.Tokens;
using ArtisanHubs.Data.Entities;
using ArtisanHubs.Data.Repositories.Accounts.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ArtisanHubs.Bussiness.Services.RefreshTokens
{
    public class RefreshTokenService : IRefreshTokenService
    {
        // Key: refreshToken, Value: AccountId
        private static readonly ConcurrentDictionary<string, int> _refreshTokens = new();

        private readonly ITokenService _tokenService;
        private readonly IAccountRepository _accountRepository;

        public RefreshTokenService(ITokenService tokenService, IAccountRepository accountRepository)
        {
            _tokenService = tokenService;
            _accountRepository = accountRepository;
        }

        // Tạo và lưu refresh token gắn với account
        public async Task<string> GenerateAndStoreRefreshToken(int accountId)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(randomBytes)
                .Replace("/", "-").Replace("+", "_").Replace("=", "");

            _refreshTokens[refreshToken] = accountId;
            return refreshToken;
        }

        // Làm mới access token bằng refresh token
        public async Task<string> RefreshAccessToken(string refreshToken)
        {
            if (!_refreshTokens.TryGetValue(refreshToken, out int accountId))
                throw new UnauthorizedAccessException("Invalid refresh token");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new UnauthorizedAccessException("Account not found");

            // Có thể tạo refresh token mới và xóa cái cũ nếu muốn bảo mật hơn
            var newAccessToken = _tokenService.CreateToken(account);

            return newAccessToken;
        }

        // Xóa refresh token (logout hoặc revoke)
        public async Task<bool> DeleteRefreshToken(string refreshToken)
        {
            return _refreshTokens.TryRemove(refreshToken, out _);
        }
    }
}
