using AutoMapper;
using EcommerceApp.Application.Features.Banners.DTOs;
using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Application.Features.Banners.Mappings;

public class BannerMappingProfile : Profile
{
    public BannerMappingProfile()
    {
        // Public banner — hides scheduling metadata
        CreateMap<Banner, BannerDto>();

        // Admin banner — exposes IsActive, StartDate, EndDate, audit timestamps
        CreateMap<Banner, AdminBannerDto>();
    }
}