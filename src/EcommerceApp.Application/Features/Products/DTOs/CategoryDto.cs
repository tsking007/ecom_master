namespace EcommerceApp.Application.Features.Products.DTOs;

/// <summary>
/// Category name with active product count.
/// Category is a simple string property on Product in this design — not a separate entity.
/// </summary>
public record CategoryDto(
    string Category,
    int Count);