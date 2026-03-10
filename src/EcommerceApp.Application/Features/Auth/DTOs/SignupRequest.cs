namespace EcommerceApp.Application.Features.Auth.DTOs;

public record SignupRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword,
    string? PhoneNumber = null
);