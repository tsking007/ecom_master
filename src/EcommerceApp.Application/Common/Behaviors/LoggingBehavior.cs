using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace EcommerceApp.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs every request entry, exit, and failure.
/// Uses structured logging — all fields are indexable in log aggregators
/// (Seq, Application Insights, ELK).
///
/// IMPORTANT: Never log raw request objects that may contain passwords,
/// OTP values, or tokens. Command/query classes should not override ToString()
/// to expose sensitive fields.
/// </summary>
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Short 8-char trace ID correlates request ↔ response in logs
        // without exposing internal IDs in production output
        var traceId = Guid.NewGuid().ToString("N")[..8];

        _logger.LogInformation(
            "[{TraceId}] Handling {RequestName}",
            traceId,
            requestName);

        TResponse response;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "[{TraceId}] Handled {RequestName} in {ElapsedMs}ms",
                traceId,
                requestName,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exceptions.ValidationException vex)
        {
            stopwatch.Stop();

            // Log validation failures at Warning level — not an application error
            _logger.LogWarning(
                "[{TraceId}] {RequestName} failed validation in {ElapsedMs}ms — " +
                "{ErrorCount} error(s): {Errors}",
                traceId,
                requestName,
                stopwatch.ElapsedMilliseconds,
                vex.Errors.Count,
                string.Join("; ", vex.Errors.SelectMany(e =>
                    e.Value.Select(msg => $"{e.Key}: {msg}"))));

            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "[{TraceId}] {RequestName} threw {ExceptionType} after {ElapsedMs}ms — {Message}",
                traceId,
                requestName,
                ex.GetType().Name,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }

        return response;
    }
}