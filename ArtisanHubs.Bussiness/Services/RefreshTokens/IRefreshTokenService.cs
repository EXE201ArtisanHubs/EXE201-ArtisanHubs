using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.RefreshTokens
{
    public interface IRefreshTokenService
    {
        Task<string> GenerateAndStoreRefreshToken(int accountId);
        Task<string> RefreshAccessToken(string refreshToken);
        Task<bool> DeleteRefreshToken(string refreshToken);
    }
}
