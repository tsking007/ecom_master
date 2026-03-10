namespace EcommerceApp.Application.Features.Auth.DTOs;

/// <summary>
/// Incoming request bodies for all Auth endpoints.
/// These are thin data containers — validation is performed by
/// FluentValidation pipeline behaviors, not data annotations.
/// </summary>




public record RefreshTokenRequest(
    string AccessToken,
    string? RefreshToken = null);   // Nullable — prefer cookie, body is fallback