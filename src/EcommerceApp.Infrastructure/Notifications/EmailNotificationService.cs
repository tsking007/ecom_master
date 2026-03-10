//using EcommerceApp.Domain.Enums;
//using EcommerceApp.Domain.Interfaces;

//namespace EcommerceApp.Infrastructure.Services;

//public class EmailNotificationService : INotificationService
//{
//    public Task SendAccountChangeOtpAsync(string email, string otp, string changeType, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendCartReminderAsync(Guid userId, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendOrderConfirmationAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendOrderStatusUpdateAsync(Guid userId, Guid orderId, TrackingStatus newStatus, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendOtpAsync(string recipient, string otp, CancellationToken cancellationToken = default)
//    {
//        // TODO: Wire up real email sending (e.g. SendGrid, SMTP)
//        Console.WriteLine($"[DEV] Sending OTP {otp} to {recipient}");
//        return Task.CompletedTask;
//    }

//    public Task SendOtpAsync(string recipient, string otp, OtpPurpose purpose, bool isSms = false, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendPasswordResetAsync(string email, string resetToken, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendPriceDropAlertAsync(Guid userId, Guid productId, decimal oldPrice, decimal newPrice, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendWelcomeAsync(Guid userId, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }
//}

//public class NotificationService : INotificationService
//{
//    public Task SendAccountChangeOtpAsync(string email, string otp, string changeType, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendCartReminderAsync(Guid userId, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendOrderConfirmationAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendOrderStatusUpdateAsync(Guid userId, Guid orderId, TrackingStatus newStatus, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendOtpAsync(string recipient, string otp, OtpPurpose purpose, bool isSms = false, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendPasswordResetAsync(string email, string resetToken, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendPriceDropAlertAsync(Guid userId, Guid productId, decimal oldPrice, decimal newPrice, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }

//    public Task SendWelcomeAsync(Guid userId, CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(true);

//    }
//}