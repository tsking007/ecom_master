using AutoMapper;
using EcommerceApp.Application.Common;
using EcommerceApp.Application.Common.Exceptions;
using EcommerceApp.Application.Features.Auth.DTOs;
using EcommerceApp.Domain.Interfaces;
using MediatR;

namespace EcommerceApp.Application.Features.Auth.Queries;

public class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated ||
            !_currentUserService.UserId.HasValue)
            throw new UnauthorizedException();

        var user = await _unitOfWork.Users
            .GetByIdAsync(
                _currentUserService.UserId.Value,
                cancellationToken)
            ?? throw new NotFoundException(
                nameof(Domain.Entities.User),
                _currentUserService.UserId.Value);

        return _mapper.Map<UserDto>(user);
    }
}