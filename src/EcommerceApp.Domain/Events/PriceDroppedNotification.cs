namespace EcommerceApp.Domain.Events;

/// <summary>
/// Published by ProductUpdatedNotification handler when NewPrice is lower than OldPrice.
/// Consumed by:
///   - PriceDropAlertService → queries Wishlist and Cart for affected users
///                             and sends price-drop-alert email + SMS
/// </summary>
public record PriceDroppedNotification(
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    decimal OldPrice,
    decimal NewPrice
) : IDomainEvent;