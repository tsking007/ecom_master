using EcommerceApp.Application.Features.Auth.DTOs;
using EcommerceApp.Domain.Enums;
using MediatR;

namespace EcommerceApp.Application.Features.Auth.Commands;

public record VerifyOtpCommand(
    string Identifier,
    string Otp,
    OtpPurpose Purpose,
    string? DeviceInfo = null,
    string? IpAddress = null
) : IRequest<VerifyOtpResponse>;