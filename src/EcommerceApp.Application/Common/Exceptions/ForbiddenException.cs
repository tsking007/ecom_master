namespace EcommerceApp.Application.Common.Exceptions;

/// <summary>
/// Thrown when the authenticated user does not have permission to perform
/// the requested action (e.g. a Customer trying to call an admin endpoint,
/// or a user trying to access another user's order).
/// The ExceptionMiddleware maps this to HTTP 403 Forbidden.
///
/// Distinct from UnauthorizedException:
///   401 = not authenticated (who are you?)
///   403 = authenticated but not allowed (you can't do this)
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("You do not have permission to perform this action.") { }

    public ForbiddenException(string message)
        : base(message) { }

    public ForbiddenException(string resourceName, Guid resourceId)
        : base($"You do not have permission to access " +
               $"{resourceName} '{resourceId}'.")
    { }
}