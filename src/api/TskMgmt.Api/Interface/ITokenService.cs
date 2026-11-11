using System.Security.Claims;
using TskMgmt.Api.Entities;

namespace TskMgmt.Api.Interface
{
    public interface ITokenService
    {
        string GenerateToken(User user);

        ClaimsPrincipal ValidateToken(string accessToken);
    }
}
