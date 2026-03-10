namespace EcommerceApp.Application.Features.Notifications.DTOs;

/// <summary>
/// Represents a single outbound SMS.
/// To must be in E.164 format: +12345678900
/// </summary>
public record SmsMessage(
    string To,
    string Body);