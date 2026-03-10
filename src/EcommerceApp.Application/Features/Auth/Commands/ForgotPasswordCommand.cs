using MediatR;

namespace EcommerceApp.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(
    string Email
) : IRequest;