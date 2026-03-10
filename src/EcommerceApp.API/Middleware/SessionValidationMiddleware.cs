using EcommerceApp.Infrastructure.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EcommerceApp.API.Middleware;

/// <summary>
/// Validates that the current session is still active in the database.
///
/// WHY:
///   JWT access tokens are stateless. If you revoke a session (logout,
///   password reset, admin suspension), the JWT remains cryptographically
///   valid until its natural expiry. Without a database check, a revoked
///   user can continue to access the API for up to 15 minutes.
///
///   This middleware adds a per-request database check on authenticated
///   requests. It reads the refresh token from the HttpOnly cookie and
///   confirms the session record exists and is not revoked.
///
/// Performance note:
///   This adds one DB read per authenticated request. For high-throughput
///   endpoints, cache the session status in Redis with a short TTL
///   (e.g., 60 seconds) to reduce DB pressure. That is wired up in Part 22.
///
/// Only runs on authenticated requests — skips anonymous endpoints entirely.
/// </summary>
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionValidationMiddleware> _logger;

    public SessionValidationMiddleware(
        RequestDelegate next,
        ILogger<SessionValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, SessionService sessionService)
    {
        // Only validate sessions for authenticated requests
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Skip session validation for the refresh and logout endpoints themselves
        // (they are authenticated but handle their own token logic)
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (path.Contains("/auth/refresh") || path.Contains("/auth/logout"))
        {
            await _next(context);
            return;
        }

        var refreshToken = context.Request.Cookies["refreshToken"];

        // If there is no refresh token cookie on an authenticated request,
        // the client is using bearer-only mode (e.g. mobile app, Postman).
        // We allow it through — the access token signature already proved validity.
        if (string.IsNullOrEmpty(refreshToken))
        {
            await _next(context);
            return;
        }

        // Parse userId from claims
        var userIdStr =
            context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
        {
            _logger.LogWarning(
                "SessionValidationMiddleware: could not parse userId from claims.");
            await _next(context);
            return;
        }

        // Look up session in DB
        var session = await sessionService.GetValidSessionAsync(
            userId, refreshToken, context.RequestAborted);

        if (session == null)
        {
            _logger.LogWarning(
                "SessionValidationMiddleware: session not found or revoked " +
                "for userId={UserId}. Returning 401.",
                userId);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                """{"statusCode":401,"message":"Session expired. Please log in again."}""");
            return;
        }

        // Touch LastRefreshedAt to keep audit log current
        await sessionService.TouchSessionAsync(session, context.RequestAborted);

        await _next(context);
    }
}