namespace EcommerceApp.Application.Features.Auth.DTOs;

public record LoginRequest(
    string Email,
    string Password,
    string? DeviceInfo = null
);