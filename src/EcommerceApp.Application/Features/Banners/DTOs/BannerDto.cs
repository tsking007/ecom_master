namespace EcommerceApp.Application.Features.Banners.DTOs;

/// <summary>
/// Banner data returned to the frontend for the Home page carousel.
/// StartDate and EndDate filtering happens in the repository — the DTO
/// never exposes scheduling metadata to public API consumers.
/// </summary>
public class BannerDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
}