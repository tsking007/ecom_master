using EcommerceApp.Application.Features.Auth.DTOs;
using MediatR;

namespace EcommerceApp.Application.Features.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password,
    string? DeviceInfo = null,
    string? IpAddress = null
) : IRequest<AuthResponse>;