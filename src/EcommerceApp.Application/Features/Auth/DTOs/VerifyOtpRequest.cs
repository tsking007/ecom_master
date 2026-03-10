using EcommerceApp.Domain.Enums;

namespace EcommerceApp.Application.Features.Auth.DTOs;

public record VerifyOtpRequest(
    string Identifier,
    string Otp,
    OtpPurpose Purpose
);