using System.ComponentModel.DataAnnotations;

namespace TskMgmt.Api.Entities
{
    public class RefreshToken
    {
        //This entity data annotations is used in db context as Fluent API configuration.
        public int Id { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get;set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public bool IsRevoked => RevokedAt.HasValue;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsActive => !IsRevoked && !IsExpired;



    }
}
