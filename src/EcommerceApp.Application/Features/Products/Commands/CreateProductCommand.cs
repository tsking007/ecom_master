using AutoMapper;
using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Products.DTOs;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Events;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Products.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

public record CreateProductCommand(
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
    bool IsFeatured,
    decimal? Weight,
    string? Dimensions
) : IRequest<ProductDto>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        // ── 1. Validate discounted price ──────────────────────────────────────
        if (command.DiscountedPrice.HasValue &&
            command.DiscountedPrice.Value >= command.Price)
            throw new ValidationException(
                "DiscountedPrice",
                "Discounted price must be less than the base price.");

        // ── 2. Generate unique slug from product name ──────────────────────────
        var baseSlug = SlugHelper.GenerateSlug(command.Name);
        var slug = baseSlug;
        var suffix = 2;

        while (await _unitOfWork.Products.SlugExistsAsync(slug, cancellationToken))
            slug = SlugHelper.AppendSuffix(baseSlug, suffix++);

        // ── 3. Create and persist ─────────────────────────────────────────────
        var product = new Product
        {
            Name = command.Name.Trim(),
            Slug = slug,
            Description = command.Description.Trim(),
            ShortDescription = command.ShortDescription?.Trim(),
            Price = command.Price,
            DiscountedPrice = command.DiscountedPrice,
            StockQuantity = command.StockQuantity,
            ReservedQuantity = 0,
            Category = command.Category.Trim(),
            SubCategory = command.SubCategory?.Trim(),
            Brand = command.Brand?.Trim(),
            ImageUrls = command.ImageUrls ?? new List<string>(),
            VideoUrl = command.VideoUrl,
            Tags = command.Tags ?? new List<string>(),
            IsFeatured = command.IsFeatured,
            IsActive = true,
            Weight = command.Weight,
            Dimensions = command.Dimensions,
            AverageRating = 0,
            ReviewCount = 0,
            SoldCount = 0
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── 4. Publish domain event (consumed by search sync service, Part 15) ─
        await _publisher.Publish(new ProductCreatedNotification(
            ProductId: product.Id,
            Name: product.Name,
            Slug: product.Slug,
            Category: product.Category,
            Price: product.EffectivePrice,
            IsActive: product.IsActive),
            cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }
}