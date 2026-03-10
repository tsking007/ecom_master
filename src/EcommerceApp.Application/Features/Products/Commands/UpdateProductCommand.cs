using AutoMapper;
using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Products.DTOs;
using EcommerceApp.Domain.Events;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Products.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

public record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string Description,
    string? ShortDescription,
    decimal Price,
    decimal? DiscountedPrice,
    int StockQuantity,
    string Category,
    string? SubCategory,
    string? Brand,
    List<string> ImageUrls,
    string? VideoUrl,
    List<string> Tags,
    bool IsActive,
    bool IsFeatured,
    decimal? Weight,
    string? Dimensions
) : IRequest<ProductDto>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        // ── 1. Load product ───────────────────────────────────────────────────
        var product = await _unitOfWork.Products.GetByIdAsync(
            command.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product", command.ProductId);

        // ── 2. Validate discounted price ──────────────────────────────────────
        if (command.DiscountedPrice.HasValue &&
            command.DiscountedPrice.Value >= command.Price)
            throw new ValidationException(
                "DiscountedPrice",
                "Discounted price must be less than the base price.");

        // ── 3. Capture old price before update (for price-drop notification) ──
        var oldPrice = product.EffectivePrice;

        // ── 4. Regenerate slug only if the name changed ───────────────────────
        if (!string.Equals(product.Name, command.Name.Trim(),
            StringComparison.OrdinalIgnoreCase))
        {
            var baseSlug = SlugHelper.GenerateSlug(command.Name);
            var slug = baseSlug;
            var suffix = 2;

            while (await _unitOfWork.Products.SlugExistsForOtherProductAsync(
                slug, product.Id, cancellationToken))
                slug = SlugHelper.AppendSuffix(baseSlug, suffix++);

            product.Slug = slug;
        }

        // ── 5. Apply updates ──────────────────────────────────────────────────
        product.Name = command.Name.Trim();
        product.Description = command.Description.Trim();
        product.ShortDescription = command.ShortDescription?.Trim();
        product.Price = command.Price;
        product.DiscountedPrice = command.DiscountedPrice;
        product.StockQuantity = command.StockQuantity;
        product.Category = command.Category.Trim();
        product.SubCategory = command.SubCategory?.Trim();
        product.Brand = command.Brand?.Trim();
        product.ImageUrls = command.ImageUrls ?? new List<string>();
        product.VideoUrl = command.VideoUrl;
        product.Tags = command.Tags ?? new List<string>();
        product.IsActive = command.IsActive;
        product.IsFeatured = command.IsFeatured;
        product.Weight = command.Weight;
        product.Dimensions = command.Dimensions;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── 6. Publish domain events ──────────────────────────────────────────
        var newPrice = product.EffectivePrice;

        await _publisher.Publish(new ProductUpdatedNotification(
            ProductId: product.Id,
            Name: product.Name,
            Slug: product.Slug,
            OldPrice: oldPrice,
            NewPrice: newPrice,
            IsActive: product.IsActive),
            cancellationToken);

        // Price dropped → notify users who wishlisted or carted this product (Part 17)
        if (newPrice < oldPrice)
            await _publisher.Publish(new PriceDroppedNotification(
                ProductId: product.Id,
                ProductName: product.Name,
                ProductSlug: product.Slug,
                OldPrice: oldPrice,
                NewPrice: newPrice),
                cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }
}