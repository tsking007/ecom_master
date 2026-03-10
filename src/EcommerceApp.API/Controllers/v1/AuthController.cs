using EcommerceApp.Application.Features.Auth.Commands;
using EcommerceApp.Application.Features.Auth.Queries;
using EcommerceApp.Application.Features.Auth.DTOs;
using EcommerceApp.Infrastructure.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EcommerceApp.API.Controllers.v1;


[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

  
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new LoginCommand(
                Email: request.Email,
                Password: request.Password,
                DeviceInfo: request.DeviceInfo
                            ?? Request.Headers.UserAgent.ToString(),
                IpAddress: GetClientIp()),
            cancellationToken);

        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(result);
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SignupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SignupResponse>> Signup(
        [FromBody] SignupRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SignupCommand(
                FirstName: request.FirstName,
                LastName: request.LastName,
                Email: request.Email,
                Password: request.Password,
                ConfirmPassword: request.ConfirmPassword,
                PhoneNumber: request.PhoneNumber),
            cancellationToken);

        return Ok(result);
    }


    [HttpPost("verify-otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VerifyOtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<VerifyOtpResponse>> VerifyOtp(
        [FromBody] VerifyOtpRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new VerifyOtpCommand(
                Identifier: request.Identifier,
                Otp: request.Otp,
                Purpose: request.Purpose,
                IpAddress: GetClientIp(),
                DeviceInfo: Request.Headers.UserAgent.ToString()),
            cancellationToken);

        // If OTP verification creates a session (e.g., EmailVerification), set cookie
        if (result.IsAuthenticated && !string.IsNullOrEmpty(result.Auth?.RefreshToken))
            SetRefreshTokenCookie(result.Auth.RefreshToken);

        return Ok(result);
    }


    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ForgotPasswordCommand(Email: request.Email),
            cancellationToken);

        return Ok(new
        {
            message = "If that email is registered, a 6-digit OTP has been sent. " +
                  "Please check your console/logs for the code."
        });
    }


    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ResetPasswordCommand(
                Token: request.Token,
                NewPassword: request.NewPassword,
                ConfirmPassword: request.ConfirmPassword),
            cancellationToken);

        return Ok(new
        {
            message = "Password reset successfully. Please log in with your new password."
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        // Prefer cookie refresh token; fall back to body (for mobile clients)
        var refreshToken =
            Request.Cookies["refreshToken"]
            ?? request.RefreshToken;

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token not provided." });

        var result = await _mediator.Send(
            new RefreshTokenCommand(
                AccessToken: request.AccessToken,
                RefreshToken: refreshToken),
            cancellationToken);

        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(result);
    }


    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userIdStr, out var userId))
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _mediator.Send(
                    new LogoutCommand(
                        UserId: userId,
                        RefreshToken: refreshToken),
                    cancellationToken);
            }
        }

        // Delete cookie regardless of whether session was found
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return NoContent();
    }

 
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCurrentUserQuery(),
            cancellationToken);

        return Ok(result);
    }


    private void SetRefreshTokenCookie(string rawRefreshToken)
    {
        Response.Cookies.Append("refreshToken", rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/api/v1/auth"   // Scope cookie only to auth endpoints
        });
    }


    private string? GetClientIp()
    {
        // X-Forwarded-For is set by reverse proxies (nginx, YARP, Azure Front Door)
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}