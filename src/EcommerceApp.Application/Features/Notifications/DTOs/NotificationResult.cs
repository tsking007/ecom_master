namespace EcommerceApp.Application.Features.Notifications.DTOs;

/// <summary>
/// Result returned by IEmailSender.SendAsync and ISmsSender.SendAsync.
/// Handlers log failures but do not throw — notification failures
/// must never prevent the primary business operation from completing.
/// </summary>
public record NotificationResult(
    bool IsSuccess,
    string? MessageId = null,
    string? ErrorMessage = null)
{
    public static NotificationResult Success(string? messageId = null)
        => new(true, messageId);

    public static NotificationResult Failure(string errorMessage)
        => new(false, null, errorMessage);
}