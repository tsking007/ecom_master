using EcommerceApp.Application.Features.Auth.DTOs;
using MediatR;

namespace EcommerceApp.Application.Features.Auth.Commands;

public record SignupCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword,
    string? PhoneNumber = null
) : IRequest<SignupResponse>;