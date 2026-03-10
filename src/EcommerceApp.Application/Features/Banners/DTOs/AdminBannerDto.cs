namespace EcommerceApp.Application.Features.Banners.DTOs;

/// <summary>
/// Admin-only banner view.
/// Extends BannerDto with scheduling and audit fields
/// that are hidden from the public /api/banners endpoint.
/// </summary>
public class AdminBannerDto : BannerDto
{
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}