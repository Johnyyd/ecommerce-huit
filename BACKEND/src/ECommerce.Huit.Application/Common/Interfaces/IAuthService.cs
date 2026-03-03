using ECommerce.Huit.Application.DTOs.Auth;

namespace ECommerce.Huit.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    Task<string?> RefreshAccessTokenAsync(string refreshToken);
}
