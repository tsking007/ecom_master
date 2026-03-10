using EcommerceApp.Domain.Common;
using EcommerceApp.Domain.Enums;
using System.Net;

namespace EcommerceApp.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? ProfileImageUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────
    public string FullName => $"{FirstName} {LastName}".Trim();

    // ── Navigation ────────────────────────────────────────────────────────────
    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; }
        = new List<Order>();
    public ICollection<Address> Addresses { get; set; }
        = new List<Address>();
    public ICollection<ProductReview> Reviews { get; set; }
        = new List<ProductReview>();
    public ICollection<Wishlist> WishlistItems { get; set; }
        = new List<Wishlist>();
    public ICollection<TokenStore> TokenStores { get; set; }
        = new List<TokenStore>();
    public ICollection<OtpStore> OtpStores { get; set; }
        = new List<OtpStore>();
    public ICollection<NotificationLog> NotificationLogs { get; set; }
        = new List<NotificationLog>();
    public ICollection<StockReservation> StockReservations { get; set; }
        = new List<StockReservation>();
}