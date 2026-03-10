namespace EcommerceApp.Domain.Interfaces;

/// <summary>
/// Generates PDF documents. Currently used for order invoices.
/// Implementation uses QuestPDF (Community license for open-source use).
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generates a PDF invoice for the given order.
    /// Returns the PDF as a byte array ready to serve as a file download
    /// or attach to an email.
    /// </summary>
    Task<byte[]> GenerateInvoiceAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);
}