namespace EcommerceApp.Application.Features.Auth.DTOs;

/// <summary>
/// Returned after a successful login or email verification.
/// The raw refresh token is returned here and should be stored in
/// an HttpOnly cookie by the API controller (Part 9).
/// </summary>
public record AuthResponse(
    UserDto User,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt
);