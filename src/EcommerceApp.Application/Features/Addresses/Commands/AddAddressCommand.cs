using AutoMapper;
using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Addresses.DTOs;
using EcommerceApp.Domain.Entities;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Addresses.Commands;

public record AddAddressCommand(
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

public class AddAddressCommandHandler
    : IRequestHandler<AddAddressCommand, AddressDto>
{
    // Maximum saved addresses per user — prevents runaway data growth
    private const int MaxAddressesPerUser = 10;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public AddAddressCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<AddressDto> Handle(
        AddAddressCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        // -- Cap check ---------------------------------------------------------
        var count = await _unitOfWork.Addresses.CountByUserIdAsync(
            userId, cancellationToken);

        if (count >= MaxAddressesPerUser)
            throw new ValidationException(
                $"You can save a maximum of {MaxAddressesPerUser} addresses.");

        // -- If new address is default, clear existing default first -----------
        if (request.IsDefault)
        {
            var existingDefault = await _unitOfWork.Addresses
                .GetDefaultAddressAsync(userId, cancellationToken);

            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
                await _unitOfWork.Addresses.UpdateAsync(
                    existingDefault, cancellationToken);
            }
        }

        // -- If this is the very first address, force it to default ------------
        var isFirstAddress = count == 0;

        var address = new Address
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = request.AddressLine2?.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim(),
            PostalCode = request.PostalCode.Trim(),
            Country = request.Country.Trim(),
            AddressType = request.AddressType.Trim(),
            IsDefault = request.IsDefault || isFirstAddress
        };

        await _unitOfWork.Addresses.AddAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AddressDto>(address);
    }
}