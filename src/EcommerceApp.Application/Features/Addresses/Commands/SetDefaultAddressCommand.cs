using AutoMapper;
using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Addresses.DTOs;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Addresses.Commands;

/// <summary>
/// Promotes one address to default and demotes the previous default.
/// Exposed as a dedicated PATCH endpoint so the frontend can change the
/// default without re-sending the full address body.
/// </summary>
public record SetDefaultAddressCommand(Guid AddressId) : IRequest<AddressDto>;

public class SetDefaultAddressCommandHandler
    : IRequestHandler<SetDefaultAddressCommand, AddressDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public SetDefaultAddressCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<AddressDto> Handle(
        SetDefaultAddressCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var address = await _unitOfWork.Addresses.GetByIdAndUserIdAsync(
            request.AddressId, userId, cancellationToken)
            ?? throw new NotFoundException("Address", request.AddressId);

        if (address.IsDefault)
            return _mapper.Map<AddressDto>(address); // already default, no-op

        // Demote current default
        var currentDefault = await _unitOfWork.Addresses
            .GetDefaultAddressAsync(userId, cancellationToken);

        if (currentDefault != null)
        {
            currentDefault.IsDefault = false;
            await _unitOfWork.Addresses.UpdateAsync(
                currentDefault, cancellationToken);
        }

        // Promote new default
        address.IsDefault = true;
        await _unitOfWork.Addresses.UpdateAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AddressDto>(address);
    }
}