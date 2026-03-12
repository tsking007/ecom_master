using AutoMapper;
using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Addresses.DTOs;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Addresses.Queries;

public record GetAddressesQuery : IRequest<IReadOnlyList<AddressDto>>;

public class GetAddressesQueryHandler
    : IRequestHandler<GetAddressesQuery, IReadOnlyList<AddressDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetAddressesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AddressDto>> Handle(
        GetAddressesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var addresses = await _unitOfWork.Addresses.GetByUserIdAsync(
            userId, cancellationToken);

        return _mapper.Map<IReadOnlyList<AddressDto>>(addresses);
    }
}