using FluentValidation.Results;

namespace EcommerceApp.Application.Common.Exceptions;

/// <summary>
/// Thrown by ValidationBehavior when one or more FluentValidation rules fail.
/// The ExceptionMiddleware maps this to HTTP 400 Bad Request with a
/// structured error body: { "errors": { "FieldName": ["error1", "error2"] } }
///
/// Can also be thrown manually in handlers for domain-level validation
/// that is too contextual for a generic FluentValidation rule.
///
/// Usage:
///   throw new ValidationException("Email", "Email address is already in use.");
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Constructs from FluentValidation failures.
    /// Groups failures by property name and collects all error messages per field.
    /// </summary>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(
                f => f.PropertyName,
                f => f.ErrorMessage)
            .ToDictionary(
                group => group.Key,
                group => group.ToArray());
    }

    /// <summary>
    /// Constructs a single-field validation error.
    /// Useful for throwing from handler code after a DB-level check.
    /// </summary>
    public ValidationException(string propertyName, string errorMessage)
        : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }

    /// <summary>
    /// Constructs a general (non-field-specific) validation error.
    /// PropertyName will be "General" in the error body.
    /// </summary>
    public ValidationException(string generalErrorMessage)
        : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { "General", new[] { generalErrorMessage } }
        };
    }
}