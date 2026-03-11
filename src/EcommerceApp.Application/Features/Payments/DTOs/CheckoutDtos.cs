namespace EcommerceApp.Application.Features.Payments.DTOs;

public class CheckoutSessionResponseDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string SessionUrl { get; set; } = string.Empty;
}

public class CheckoutSessionStatusDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;          // paid | pending | failed
    public string PaymentStatus { get; set; } = string.Empty;   // domain payment status
    public string TrackingStatus { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public string? StripeSessionStatus { get; set; }
    public string? StripePaymentStatus { get; set; }
}