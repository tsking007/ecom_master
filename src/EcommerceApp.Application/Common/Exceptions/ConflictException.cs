namespace EcommerceApp.Application.Common.Exceptions;

/// <summary>
/// Thrown when a create or update operation conflicts with existing data.
/// Common cases:
///   - Registering with an email that already exists
///   - Creating a product with a slug that is already in use
///   - A user trying to review a product they have already reviewed
/// The ExceptionMiddleware maps this to HTTP 409 Conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message) { }

    public ConflictException(string resourceName, string conflictDetail)
        : base($"{resourceName} conflict: {conflictDetail}.") { }
}