using AutoMapper;
using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Addresses.DTOs;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Addresses.Commands;

public record UpdateAddressCommand(
    Guid AddressId,
    string FullName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    string AddressType,
    bool IsDefault
) : IRequest<AddressDto>;

public class UpdateAddressCommandHandler
    : IRequestHandler<UpdateAddressCommand, AddressDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateAddressCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<AddressDto> Handle(
        UpdateAddressCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var address = await _unitOfWork.Addresses.GetByIdAndUserIdAsync(
            request.AddressId, userId, cancellationToken)
            ?? throw new NotFoundException("Address", request.AddressId);

        // -- If promoting this address to default, demote the old one ----------
        if (request.IsDefault && !address.IsDefault)
        {
            var existingDefault = await _unitOfWork.Addresses
                .GetDefaultAddressAsync(userId, cancellationToken);

            if (existingDefault != null && existingDefault.Id != address.Id)
            {
                existingDefault.IsDefault = false;
                await _unitOfWork.Addresses.UpdateAsync(
                    existingDefault, cancellationToken);
            }
        }

        // -- Apply changes -----------------------------------------------------
        address.FullName = request.FullName.Trim();
        address.PhoneNumber = request.PhoneNumber.Trim();
        address.AddressLine1 = request.AddressLine1.Trim();
        address.AddressLine2 = request.AddressLine2?.Trim();
        address.City = request.City.Trim();
        address.State = request.State.Trim();
        address.PostalCode = request.PostalCode.Trim();
        address.Country = request.Country.Trim();
        address.AddressType = request.AddressType.Trim();
        address.IsDefault = request.IsDefault;

        await _unitOfWork.Addresses.UpdateAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AddressDto>(address);
    }
}