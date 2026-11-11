using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TskMgmt.Api.Context;
using TskMgmt.Api.Entities;
using TskMgmt.Api.Entities.Dto;
using TskMgmt.Api.Interface;

namespace TskMgmt.Api.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly PasswordHasher<Entities.User> _passwordHasher;
        private readonly RefreshTokenService _refreshTokenService;

        public AuthService(AppDbContext context, ITokenService tokenService, RefreshTokenService refreshTokenService)
        {
            _context = context;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _passwordHasher = new PasswordHasher<Entities.User>();
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email);

            if(existingUser)
                throw new Exception("User with this email already exists.");

            var user = new Entities.User
            {
                Name = request.Name,
                Email = request.Email,
            };

             user.Password = _passwordHasher.HashPassword(user, request.Password);
            
            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return await CreateAuthResponse(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new Exception("Invalid email or password.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

            if(result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid email or password.");


            return await CreateAuthResponse(user);
        }

        public async System.Threading.Tasks.Task LogoutAsync(string token, string refreshToken, string userId)
        {
            var principal = _tokenService.ValidateToken(token);

            if (principal == null)
                throw new UnauthorizedAccessException("Invalid Access Token");

            var tokenId = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(tokenId))
                throw new UnauthorizedAccessException("Invalid token.");

            var expiration = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

            if(string.IsNullOrEmpty(expiration))
                throw new UnauthorizedAccessException("Invalid token expiration.");

            var expirationDateTime = DateTimeOffset
                .FromUnixTimeSeconds(long.Parse(expiration))
                .UtcDateTime;
            
            var tokenUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (tokenUserId != userId)
                throw new UnauthorizedAccessException("User Mismatch");

            var tokenHash = _refreshTokenService.HashToken(refreshToken);


            var handler = new JwtSecurityTokenHandler();
            
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var alreadyRevokedTokens = await _context.RevokedTokens
                  .AnyAsync(rt => rt.TokenId == tokenId);

            if(!alreadyRevokedTokens)
            {
                _context.RevokedTokens.Add(new Entities.RevokedToken
                {
                    TokenId = tokenId,
                    ExpiresAt = expirationDateTime,
                    RevokedAt = DateTime.UtcNow
                });
            }

            var refreshTokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.UserId == userId);

            if(refreshTokenEntity != null && !refreshTokenEntity.IsRevoked)
            {
                refreshTokenEntity.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

        }

        public async Task<AuthResponse> RefreshTokenAsync(string token)
        {
            var tokenHash = _refreshTokenService.HashToken(token);

            var existingToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

            if (existingToken == null)
                return null;

            if (existingToken.IsRevoked)
            {
                var activeTokens = await _context.RefreshTokens
                    .Where(x => x.UserId == existingToken.UserId
                        && x.RevokedAt == null)
                    .ToListAsync();

                foreach (var activeToken in activeTokens)
                    activeToken.RevokedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return null;
            }

            if (existingToken.IsExpired)
                return null;

            var user = await _context.Users.FindAsync(
                Guid.Parse(existingToken.UserId));

            if (user == null)
                return null;

            var newAccessToken = _tokenService.GenerateToken(user);
            var newRefreshToken = _refreshTokenService.GenerateToken();

            var newRefreshTokenHash =
                _refreshTokenService.HashToken(newRefreshToken);

            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.ReplacedByTokenHash = newRefreshTokenHash;

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id.ToString(),
                TokenHash = newRefreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        private async Task<AuthResponse> CreateAuthResponse(Entities.User user)
        {
            var accessToken = _tokenService.GenerateToken(user);

            var refreshToken = _refreshTokenService.GenerateToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id.ToString(),
                TokenHash = _refreshTokenService.HashToken(refreshToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
                RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
            };

        }
    }
}
