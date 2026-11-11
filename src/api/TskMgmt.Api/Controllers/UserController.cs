using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TskMgmt.Api.Interface;

namespace TskMgmt.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
       

       public UserController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Entities.Dto.RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Entities.Dto.LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] Entities.Dto.LogoutRequest request)
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();

            var token = authorizationHeader.Replace(
                "Bearer ", 
                "",
                StringComparison.OrdinalIgnoreCase
                ).Trim();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _authService.LogoutAsync(token, request.RefreshToken, userId);

            return Ok(new {message = "Logged out successfully"});

        }


        [Authorize]
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(string token)
        {
           
            var refreshToken = await _authService.RefreshTokenAsync(token);
            return Ok(refreshToken);
        }


    }
}
