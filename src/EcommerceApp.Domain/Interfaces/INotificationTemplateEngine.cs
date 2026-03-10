namespace EcommerceApp.Domain.Interfaces;

/// <summary>
/// Renders a named Scriban template file with the supplied model object.
/// Results are cached after the first render.
/// </summary>
public interface INotificationTemplateEngine
{
    /// <summary>
    /// templateName — filename without extension, e.g. "otp-email" or "cart-reminder-sms".
    /// The implementation appends .html for email channels and .txt for SMS channels.
    /// model — anonymous object or typed class whose properties map to template variables.
    /// Returns the fully rendered string (HTML or plain text).
    /// </summary>
    Task<string> RenderAsync(
        string templateName,
        object model,
        CancellationToken cancellationToken = default);
}