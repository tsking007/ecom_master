using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class Address : BaseEntity
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;

    // "Home" | "Work" | "Other"
    public string AddressType { get; set; } = "Home";

    // ── Navigation ────────────────────────────────────────────────────────────
    public User User { get; set; } = null!;
}