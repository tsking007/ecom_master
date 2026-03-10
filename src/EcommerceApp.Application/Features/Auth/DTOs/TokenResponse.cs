namespace EcommerceApp.Application.Features.Auth.DTOs;

/// <summary>
/// Returned after a successful token refresh.
/// Only contains the new tokens — no user data (saves a DB round-trip).
/// </summary>
public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt
);