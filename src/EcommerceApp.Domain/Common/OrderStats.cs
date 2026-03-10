namespace EcommerceApp.Domain.Common;

public record OrderStats(
    decimal TotalRevenue,
    int TotalOrders,
    int PendingOrders,
    int ProcessingOrders,
    int ShippedOrders,
    int DeliveredOrders,
    int CancelledOrders,
    decimal AverageOrderValue
);