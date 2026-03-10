namespace EcommerceApp.Application.Features.Auth.DTOs;

/// <summary>
/// Returned after a successful signup request.
/// Tells the frontend which email address to show on the OTP entry screen.
/// </summary>
public record SignupResponse(
    string Email,
    string Message
);