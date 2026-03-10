namespace EcommerceApp.Application.Common.Exceptions;

/// <summary>
/// Thrown when the request does not carry a valid authentication token
/// or the session has expired.
/// The ExceptionMiddleware maps this to HTTP 401 Unauthorized.
///
/// Distinct from ForbiddenException:
///   401 = not authenticated (who are you?)
///   403 = authenticated but not allowed (you can't do this)
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base("You are not authenticated. Please log in and try again.") { }

    public UnauthorizedException(string message)
        : base(message) { }
}