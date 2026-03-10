namespace EcommerceApp.Domain.Enums;

public enum OtpPurpose
{
    EmailVerification = 1,
    PhoneVerification = 2,
    ForgotPassword = 3,
    ChangeEmail = 4,
    ChangePhone = 5,
    TwoFactorAuth = 6
}