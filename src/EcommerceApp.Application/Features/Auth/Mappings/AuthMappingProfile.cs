using AutoMapper;
using EcommerceApp.Application.Features.Auth.DTOs;
using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Application.Features.Auth.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, UserDto>()
            // FullName is a computed C# property on User — map directly
            .ForMember(d => d.FullName,
                opt => opt.MapFrom(s => s.FullName))
            // Enum to string — "Customer" or "Admin"
            .ForMember(d => d.Role,
                opt => opt.MapFrom(s => s.Role.ToString()));
    }
}