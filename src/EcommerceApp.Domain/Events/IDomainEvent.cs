using MediatR;

namespace EcommerceApp.Domain.Events;

/// <summary>
/// Marker interface for all domain events.
/// Extends INotification so MediatR can dispatch events
/// directly without any adapter layer.
/// </summary>
public interface IDomainEvent : INotification
{
}