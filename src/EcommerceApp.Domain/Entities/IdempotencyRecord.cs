using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class IdempotencyRecord : BaseEntity
{
    // The unique key sent by the client (e.g. "idem_userId_cartHash_timestamp")
    public string IdempotencyKey { get; set; } = string.Empty;

    // The userId who owns this record — prevents key collision across users
    public Guid UserId { get; set; }

    // Cached JSON response so we can return the exact same DTO on replay
    public string ResponseJson { get; set; } = string.Empty;

    // The Stripe session ID — used to detect if Stripe already has this session
    public string StripeSessionId { get; set; } = string.Empty;

    // Keys expire after 24 hours — after that a fresh session can be created
    public DateTime ExpiresAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}