using MediatR;

namespace EcommerceApp.Application.Features.Auth.Commands;

public record LogoutCommand(
    Guid UserId,
    string RefreshToken
) : IRequest;