namespace EcommerceApp.Infrastructure.Notifications;

/// <summary>
/// Bound to the "Notifications" section in appsettings.
/// Injected into all notification command handlers via IOptions&lt;NotificationSettings&gt;.
/// </summary>
public class NotificationSettings
{
    public const string SectionName = "Notifications";

    /// <summary>From address shown in email clients.</summary>
    public string FromEmail { get; init; } = string.Empty;

    /// <summary>Display name shown alongside FromEmail.</summary>
    public string FromName { get; init; } = "EcommerceApp";

    /// <summary>
    /// E.164 from-number for outbound SMS (e.g. +12015550123).
    /// Used by TwilioSmsSender in Part 12.
    /// </summary>
    public string FromPhone { get; init; } = string.Empty;

    /// <summary>
    /// Base URL of the frontend app — used to construct reset password links.
    /// Example: https://app.ecommerceapp.com
    /// </summary>
    public string FrontendBaseUrl { get; init; } = "http://localhost:3000";
}