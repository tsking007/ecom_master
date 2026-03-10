using EcommerceApp.Application.Common.Interfaces;
using EcommerceApp.Application.Features.Notifications.Templates;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EcommerceApp.Infrastructure.Notifications;

namespace EcommerceApp.Application.Features.Notifications.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>
/// Dispatched by PlaceOrderCommandHandler (Part 20).
/// Fleshed out with full order line items in Part 20.
/// </summary>
public record SendOrderConfirmationEmailCommand(
    string To,
    string FirstName,
    string OrderNumber,
    decimal OrderTotal) : IRequest;

// ── Handler ───────────────────────────────────────────────────────────────────

public class SendOrderConfirmationEmailCommandHandler
    : IRequestHandler<SendOrderConfirmationEmailCommand>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationSettings _settings;
    private readonly ILogger<SendOrderConfirmationEmailCommandHandler> _logger;

    public SendOrderConfirmationEmailCommandHandler(
        IEmailSender emailSender,
        IOptions<NotificationSettings> settings,
        ILogger<SendOrderConfirmationEmailCommandHandler> logger)
    {
        _emailSender = emailSender;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task Handle(
        SendOrderConfirmationEmailCommand command,
        CancellationToken cancellationToken)
    {
        var message = EmailTemplateEngine.BuildOrderConfirmationEmail(
            to: command.To,
            firstName: command.FirstName,
            orderNumber: command.OrderNumber,
            orderTotal: command.OrderTotal,
            fromEmail: _settings.FromEmail,
            fromName: _settings.FromName);

        var result = await _emailSender.SendAsync(message, cancellationToken);

        if (!result.IsSuccess)
            _logger.LogWarning(
                "Order confirmation email failed for order #{OrderNumber}: {Error}",
                command.OrderNumber, result.ErrorMessage);
    }
}