using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class TokenStore : BaseEntity
{
    public Guid UserId { get; set; }

    // Hashed refresh token value — never store raw
    public string RefreshToken { get; set; } = string.Empty;

    // Browser / OS info captured at login
    public string? DeviceInfo { get; set; }

    public string? IpAddress { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastRefreshedAt { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public User User { get; set; } = null!;
}