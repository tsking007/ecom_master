namespace EcommerceApp.Domain.Interfaces;

public interface ISmsService
{
    /// <summary>
    /// Sends a plain-text SMS to the given phone number.
    /// Phone number must include country code e.g. +91XXXXXXXXXX
    /// </summary>
    Task<bool> SendAsync(
        string to,
        string message,
        CancellationToken cancellationToken = default);
}