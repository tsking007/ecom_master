namespace EcommerceApp.Application.Features.Auth.DTOs;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool IsEmailVerified,
    bool IsPhoneVerified,
    bool IsActive,
    string? ProfileImageUrl,
    DateTime? DateOfBirth,
    DateTime CreatedAt
);