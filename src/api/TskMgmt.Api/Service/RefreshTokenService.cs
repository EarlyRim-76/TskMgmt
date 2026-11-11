using System.Security.Cryptography;
using System.Text;

namespace TskMgmt.Api.Service
{
    public class RefreshTokenService
    {
        public string GenerateToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(randomBytes);
        }

        public string HashToken(string token)
        {
           var bytes =  Encoding.UTF8.GetBytes(token);

            var hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash);
        }
    }
}
