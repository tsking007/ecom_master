using AutoMapper;
using EcommerceApp.Application.Features.Addresses.DTOs;
using EcommerceApp.Domain.Entities;

namespace EcommerceApp.Application.Features.Addresses.Mappings;

public class AddressMappingProfile : Profile
{
    public AddressMappingProfile()
    {
        CreateMap<Address, AddressDto>();
    }
}