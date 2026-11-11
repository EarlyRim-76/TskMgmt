using TskMgmt.Api.Entities;
using TskMgmt.Api.Entities.Dto;

namespace TskMgmt.Api.Interface
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<AuthResponse> RefreshTokenAsync(string token);

        System.Threading.Tasks.Task LogoutAsync(string token, string refreshToken, string userId);

    }
}
