using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Auth;
using ECommerce.Huit.Application.Validators.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Huit.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { error = "ValidationFailed", details = validation.Errors });

        var result = await _authService.RegisterAsync(request);
        if (result == null)
            return Conflict(new { error = "EmailOrPhoneExists" });

        return CreatedAtAction(nameof(Login), new { email = request.Email }, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var validation = await _loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { error = "ValidationFailed", details = validation.Errors });

        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(new { error = "InvalidCredentials" });

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var newToken = await _authService.RefreshAccessTokenAsync(request.RefreshToken);
        if (string.IsNullOrEmpty(newToken))
            return Unauthorized(new { error = "InvalidRefreshToken" });

        return Ok(new { access_token = newToken });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Headers["X-Refresh-Token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.RevokeRefreshTokenAsync(refreshToken);
        }

        return NoContent();
    }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
