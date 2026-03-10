using EcommerceApp.Domain.Common;

namespace EcommerceApp.Domain.Entities;

public class Banner : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    // Destination URL when the banner is clicked (null = not clickable)
    public string? LinkUrl { get; set; }

    // Lower number = shown first in the carousel
    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    // Optional scheduling — null means always show (when IsActive = true)
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}