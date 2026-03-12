using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Addresses.Commands;

public record DeleteAddressCommand(Guid AddressId) : IRequest;

public class DeleteAddressCommandHandler
    : IRequestHandler<DeleteAddressCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteAddressCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(
        DeleteAddressCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var address = await _unitOfWork.Addresses.GetByIdAndUserIdAsync(
            request.AddressId, userId, cancellationToken)
            ?? throw new NotFoundException("Address", request.AddressId);

        // ✅ Correct method name from IRepository<T>
        await _unitOfWork.Addresses.HardDeleteAsync(address, cancellationToken);

        // If the deleted address was the default, promote the next one
        if (address.IsDefault)
        {
            var remaining = await _unitOfWork.Addresses
                .GetByUserIdAsync(userId, cancellationToken);

            // SoftDelete sets IsDeleted = true and marks entity as Modified,
            // but SaveChanges hasn't been called yet, so we filter it out manually
            var next = remaining.FirstOrDefault(a => a.Id != address.Id);
            if (next != null)
            {
                next.IsDefault = true;
                await _unitOfWork.Addresses.UpdateAsync(next, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}