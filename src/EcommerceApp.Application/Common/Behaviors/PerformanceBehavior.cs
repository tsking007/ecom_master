using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EcommerceApp.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that measures handler execution time and
/// logs a structured warning if the handler exceeds WarningThresholdMs.
///
/// This catches slow EF Core queries, missing indexes, or expensive
/// computations before they become visible to end users.
///
/// The threshold is intentionally low (500ms) — tune per handler if needed
/// by adjusting the constant or reading from IConfiguration.
/// </summary>
public class PerformanceBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int WarningThresholdMs = 500;

    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (elapsedMs > WarningThresholdMs)
        {
            _logger.LogWarning(
                "SLOW HANDLER — {RequestName} took {ElapsedMs}ms " +
                "(warning threshold: {ThresholdMs}ms). " +
                "Check for missing indexes, N+1 queries, or missing caching.",
                typeof(TRequest).Name,
                elapsedMs,
                WarningThresholdMs);
        }

        return response;
    }
}