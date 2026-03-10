using AutoMapper;
using EcommerceApp.Application.Features.Products.DTOs;
using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Application.Features.Products.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // ── Product → ProductDto (detail page) ────────────────────────────────
        // AutoMapper maps same-name properties automatically.
        // Computed properties (EffectivePrice, DiscountPercentage, AvailableStock)
        // are readable on Product so they map by convention.
        // Only MainImageUrl needs a custom mapping.
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.EffectivePrice,
                o => o.MapFrom(s => s.EffectivePrice))
            .ForMember(d => d.DiscountPercentage,
                o => o.MapFrom(s => s.DiscountPercentage))
            .ForMember(d => d.AvailableStock,
                o => o.MapFrom(s => s.AvailableStock))
            .ForMember(d => d.MainImageUrl,
                o => o.MapFrom(s => s.ImageUrls.FirstOrDefault()));

        // ── Product → ProductListDto (listing / card) ─────────────────────────
        CreateMap<Product, ProductListDto>()
            .ForMember(d => d.EffectivePrice,
                o => o.MapFrom(s => s.EffectivePrice))
            .ForMember(d => d.DiscountPercentage,
                o => o.MapFrom(s => s.DiscountPercentage))
            .ForMember(d => d.AvailableStock,
                o => o.MapFrom(s => s.AvailableStock))
            .ForMember(d => d.MainImageUrl,
                o => o.MapFrom(s => s.ImageUrls.FirstOrDefault()));
    }
}