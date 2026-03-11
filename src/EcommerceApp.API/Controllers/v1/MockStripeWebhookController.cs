using EcommerceApp.Application.Features.Payments.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/payments/mock")]
public class MockStripeWebhookController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IWebHostEnvironment _environment;

    public MockStripeWebhookController(
        ISender sender,
        IWebHostEnvironment environment)
    {
        _sender = sender;
        _environment = environment;
    }

    [HttpPost("checkout-session-completed/{sessionId}")]
    [AllowAnonymous]
    public async Task<IActionResult> MockCheckoutCompleted(
        string sessionId,
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        await _sender.Send(
            new MarkCheckoutSessionCompletedCommand(sessionId),
            cancellationToken);

        return Ok(new
        {
            message = "Mock checkout.session.completed processed.",
            sessionId
        });
    }

    [HttpPost("payment-failed/{orderId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> MockPaymentFailed(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        await _sender.Send(
            new MarkPaymentFailedCommand(
                orderId,
                "mock_payment_intent",
                "Mock payment failure from local development endpoint."),
            cancellationToken);

        return Ok(new
        {
            message = "Mock payment_intent.payment_failed processed.",
            orderId
        });
    }
}