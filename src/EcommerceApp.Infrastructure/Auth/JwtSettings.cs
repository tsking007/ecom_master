namespace EcommerceApp.Infrastructure.Auth;

/// <summary>
/// Strongly-typed options class bound to the "Jwt" section in appsettings.
/// Registered with services.Configure&lt;JwtSettings&gt;(configuration.GetSection("Jwt"))
/// in DependencyInjection.cs and injected via IOptions&lt;JwtSettings&gt;.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// HMAC-SHA256 signing secret. Must be at least 32 characters.
    /// Store in environment variables or Azure Key Vault — never commit to source control.
    /// </summary>
    public string Secret { get; init; } = string.Empty;

    /// <summary>Token issuer — typically the API base URL or app name.</summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>Token audience — typically the frontend app or API name.</summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>Access token lifetime in minutes. Default: 15.</summary>
    public int AccessTokenExpiryMinutes { get; init; } = 15;

    /// <summary>Refresh token lifetime in days. Default: 7.</summary>
    public int RefreshTokenExpiryDays { get; init; } = 7;

    /// <summary>Password reset token lifetime in minutes. Default: 10.</summary>
    public int PasswordResetTokenExpiryMinutes { get; init; } = 10;
}