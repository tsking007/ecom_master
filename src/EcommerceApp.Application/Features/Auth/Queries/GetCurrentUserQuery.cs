using EcommerceApp.Application.Features.Auth.DTOs;
using MediatR;

namespace EcommerceApp.Application.Features.Auth.Queries;

public record GetCurrentUserQuery : IRequest<UserDto>;