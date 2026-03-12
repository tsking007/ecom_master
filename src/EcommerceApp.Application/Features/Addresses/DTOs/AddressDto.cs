namespace EcommerceApp.Application.Features.Addresses.DTOs;

public class AddressDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    // "Home" | "Work" | "Other"
    public string AddressType { get; set; } = "Home";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}