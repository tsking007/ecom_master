namespace EcommerceApp.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity cannot be found in the database.
/// The ExceptionMiddleware maps this to HTTP 404 Not Found.
///
/// Usage:
///   throw new NotFoundException(nameof(Product), productId);
///   throw new NotFoundException("Product with this slug was not found.");
/// </summary>
public class NotFoundException : Exception
{
    public string ResourceName { get; }
    public object ResourceKey { get; }

    public NotFoundException(string resourceName, object resourceKey)
        : base($"{resourceName} with key '{resourceKey}' was not found.")
    {
        ResourceName = resourceName;
        ResourceKey = resourceKey;
    }

    public NotFoundException(string message)
        : base(message)
    {
        ResourceName = "Resource";
        ResourceKey = string.Empty;
    }
}