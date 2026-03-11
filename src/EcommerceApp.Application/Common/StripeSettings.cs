namespace EcommerceApp.Application.Common;

public class StripeSettings
{
    public const string SectionName = "Stripe";

    public string PublishableKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
    public string Currency { get; init; } = "usd";
    public string SuccessUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;
}