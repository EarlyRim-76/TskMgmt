namespace TskMgmt.Api.Entities
{
    public class RevokedToken
    {
        public int Id { get; set; }

        public string TokenId { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
    }
}
