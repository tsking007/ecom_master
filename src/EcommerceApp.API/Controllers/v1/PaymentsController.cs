using EcommerceApp.Application.Common;
using EcommerceApp.Application.Features.Payments.Commands;
using EcommerceApp.Application.Features.Payments.DTOs;
using EcommerceApp.Application.Features.Payments.Queries;
using EcommerceApp.Infrastructure.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Text;

namespace EcommerceApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        ISender sender,
        IOptions<StripeSettings> stripeOptions,
        ILogger<PaymentsController> logger)
    {
        _sender = sender;
        _stripeSettings = stripeOptions.Value;
        _logger = logger;
    }

    [HttpPost("create-checkout-session")]
    [Authorize]
    [ProducesResponseType(typeof(CheckoutSessionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CheckoutSessionResponseDto>> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateCheckoutSessionCommand(request.AddressId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("session/{sessionId}")]
    [Authorize]
    [ProducesResponseType(typeof(CheckoutSessionStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CheckoutSessionStatusDto>> GetSessionStatus(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCheckoutSessionStatusQuery(sessionId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        var json = await new StreamReader(HttpContext.Request.Body, Encoding.UTF8)
            .ReadToEndAsync(cancellationToken);

        var signature = Request.Headers["Stripe-Signature"];

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                signature,
                _stripeSettings.WebhookSecret);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid Stripe webhook signature.");
            return BadRequest();
        }

        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session != null)
                    {
                        await _sender.Send(
                            new MarkCheckoutSessionCompletedCommand(
                                session.Id,
                                session.PaymentIntentId),
                            cancellationToken);
                    }
                    break;
                }

            case "payment_intent.payment_failed":
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null &&
                        paymentIntent.Metadata.TryGetValue("orderId", out var orderIdRaw) &&
                        Guid.TryParse(orderIdRaw, out var orderId))
                    {
                        await _sender.Send(
                            new MarkPaymentFailedCommand(
                                orderId,
                                paymentIntent.Id,
                                paymentIntent.LastPaymentError?.Message),
                            cancellationToken);
                    }
                    break;
                }

            case "payment_intent.succeeded":
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        _logger.LogInformation(
                            "Stripe payment_intent.succeeded received. PaymentIntentId={PaymentIntentId}",
                            paymentIntent.Id);
                    }
                    break;
                }

            default:
                _logger.LogInformation("Unhandled Stripe webhook event type: {EventType}", stripeEvent.Type);
                break;
        }

        return Ok(new { received = true });
    }

    public class CreateCheckoutSessionRequest
    {
        public Guid? AddressId { get; set; }
    }
}