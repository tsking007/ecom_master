using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Domain.Interfaces;

public interface INotificationService
{

    Task SendOtpAsync(
        string recipient,
        string otp,
        OtpPurpose purpose,
        bool isSms = false,
        CancellationToken cancellationToken = default);

    Task SendWelcomeAsync(
    Guid userId,
    CancellationToken cancellationToken = default);

    //Task SendOrderConfirmationAsync(
    //    Guid userId,
    //    Guid orderId,
    //    CancellationToken cancellationToken = default);

    //Task SendCartReminderAsync(
    //    Guid userId,
    //    CancellationToken cancellationToken = default);

    //Task SendPriceDropAlertAsync(
    //    Guid userId,
    //    Guid productId,
    //    decimal oldPrice,
    //    decimal newPrice,
    //    CancellationToken cancellationToken = default);

    //Task SendPasswordResetAsync(
    //    string email,
    //    string resetToken,
    //    CancellationToken cancellationToken = default);


    //Task SendOrderStatusUpdateAsync(
    //    Guid userId,
    //    Guid orderId,
    //    TrackingStatus newStatus,
    //    CancellationToken cancellationToken = default);


    //Task SendAccountChangeOtpAsync(
    //    string email,
    //    string otp,
    //    string changeType,
    //    CancellationToken cancellationToken = default);
}