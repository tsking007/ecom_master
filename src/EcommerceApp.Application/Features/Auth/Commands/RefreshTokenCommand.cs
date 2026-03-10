using EcommerceApp.Application.Features.Auth.DTOs;
using MediatR;

namespace EcommerceApp.Application.Features.Auth.Commands;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken
) : IRequest<TokenResponse>;