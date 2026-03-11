using EcommerceApp.Application.Common.Exceptions;

namespace EcommerceApp.API.Middleware;

/// <summary>
/// Global unhandled exception handler.
///
/// Maps known Application-layer exception types to appropriate HTTP status codes.
/// Any unrecognised exception falls through to 500 Internal Server Error.
///
/// Exception type → HTTP status mapping:
///   ValidationException      → 400 Bad Request
///   NotFoundException        → 404 Not Found
///   ConflictException        → 409 Conflict
///   UnauthorizedException    → 401 Unauthorized
///   ForbiddenException       → 403 Forbidden
///   (anything else)          → 500 Internal Server Error
///
/// The full stack trace is NEVER sent to the client in production.
/// It is logged via ILogger and readable in Application Insights / Seq.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception on {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (400, "Validation failed.", ve.Errors),
            NotFoundException nfe => (404, nfe.Message, null),
            ConflictException ce => (409, ce.Message, null),
            UnauthorizedException ue => (401, ue.Message, null),
            ForbiddenException fe => (403, fe.Message, null),
            RateLimitExceededException re => (429, re.Message, null),
            _ => (500, "An unexpected error occurred.", null)
        };

        if (exception is RateLimitExceededException rle && rle.RetryAfter.HasValue)
        {
            context.Response.Headers["Retry-After"] =
                ((int)rle.RetryAfter.Value.TotalSeconds).ToString();
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            StatusCode = statusCode,
            Message = message,
            Errors = errors
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}