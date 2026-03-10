using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Domain.Events;

/// <summary>
/// Published when an admin changes the tracking or payment status of an order.
/// Consumed by:
///   - NotificationService → sends order-status-update email and SMS to the customer
/// </summary>
public record OrderStatusChangedNotification(
    Guid OrderId,
    Guid UserId,
    string OrderNumber,
    TrackingStatus OldStatus,
    TrackingStatus NewStatus
) : IDomainEvent;