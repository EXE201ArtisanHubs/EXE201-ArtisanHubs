using ArtisanHubs.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Bussiness.Services.Tokens
{
    public interface ITokenService
    {
        string CreateToken(Account account);
    }
}
