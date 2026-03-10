using MediatR;

namespace EcommerceApp.Application.Features.Auth.Commands;

public record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword
) : IRequest;