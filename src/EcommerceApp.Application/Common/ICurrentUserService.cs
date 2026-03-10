namespace EcommerceApp.Application.Common;

/// <summary>
/// Abstracts the current authenticated user's identity.
/// Implemented in the API layer using IHttpContextAccessor and JWT claims.
/// A no-op stub is used by the EF Core design-time factory (migrations).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>The authenticated user's ID. Null for anonymous requests.</summary>
    Guid? UserId { get; }

    /// <summary>True when the current request carries a valid JWT.</summary>
    bool IsAuthenticated { get; }

    /// <summary>The authenticated user's email claim.</summary>
    string? Email { get; }

    /// <summary>"Admin" or "Customer".</summary>
    string? Role { get; }
}