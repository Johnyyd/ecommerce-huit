using System.Threading.Tasks;
using HuitShopDB.Models.DTOs.Auth;

namespace HuitShopDB.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
        Task<string> RefreshAccessTokenAsync(string refreshToken);
    }
}

