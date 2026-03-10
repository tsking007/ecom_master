namespace EcommerceApp.API.Middleware;

/// <summary>
/// Assigns a correlation ID to every HTTP request.
///
/// WHY:
///   When a request fails, the error response contains an X-Correlation-ID
///   header. The user (or support team) can provide this ID and you can grep
///   logs to trace every log entry for that specific request — across
///   multiple services if needed.
///
/// Behaviour:
///   1. If the incoming request already has X-Correlation-ID (set by a gateway
///      or upstream service), that value is kept and propagated.
///   2. If not, a new GUID is generated and assigned.
///   3. The ID is added to the response headers so the client can log it.
///   4. The ID is added to the logging scope so every Serilog entry for this
///      request includes {CorrelationId} automatically.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Read existing or generate new
        var correlationId =
            context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        // 2. Store in HttpContext.Items for access anywhere in the request
        context.Items["CorrelationId"] = correlationId;

        // 3. Echo back in response headers so clients can log it
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // 4. Add to the logging scope — every log entry in this request
        //    will automatically include CorrelationId={correlationId}
        using (_logger.BeginScope(
            new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }
}