namespace EcommerceApp.Application.Features.Auth.DTOs;

/// <summary>
/// Returned after OTP verification. The populated fields depend on the purpose:
///
///   EmailVerification → IsAuthenticated = true,  Auth = AuthResponse
///   ForgotPassword    → IsAuthenticated = false, PasswordResetToken = JWT
///   PhoneVerification → IsAuthenticated = false, Message = "Phone verified"
///   ChangeEmail       → IsAuthenticated = false, Message = "Email updated"
///   ChangePhone       → IsAuthenticated = false, Message = "Phone updated"
/// </summary>
public record VerifyOtpResponse(
    bool IsAuthenticated,
    AuthResponse? Auth,
    string? PasswordResetToken,
    string Message
);