namespace EcommerceApp.Domain.Enums;

public enum TrackingStatus
{
    Placed = 1,
    Processing = 2,
    Confirmed = 3,
    Shipped = 4,
    OutForDelivery = 5,
    Delivered = 6,
    Cancelled = 7,
    ReturnRequested = 8,
    ReturnApproved = 9,
    ReturnRejected = 10,
    Refunded = 11
}