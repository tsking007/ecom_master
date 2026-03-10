using EcommerceApp.Application.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EcommerceApp.API.Services;

/// <summary>
/// Reads the authenticated user's identity from the current HTTP request's
/// ClaimsPrincipal (populated by the JWT bearer middleware).
///
/// Registered as Scoped — a new instance per HTTP request.
/// Injected into AppDbContext (for audit fields) and Application handlers
/// (for ownership checks).
///
/// Returns null/false for all properties on unauthenticated requests.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal =>
        _httpContextAccessor.HttpContext?.User;

    // ── ICurrentUserService ───────────────────────────────────────────────────

    public Guid? UserId
    {
        get
        {
            var claim =
                Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated ?? false;

    public string? Email =>
        Principal?.FindFirst(JwtRegisteredClaimNames.Email)?.Value
        ?? Principal?.FindFirst(ClaimTypes.Email)?.Value;

    public string? Role =>
        Principal?.FindFirst(ClaimTypes.Role)?.Value;
}