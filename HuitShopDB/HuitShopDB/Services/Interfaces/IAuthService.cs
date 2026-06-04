using HuitShopDB.Models.DTOs.Auth;

namespace HuitShopDB.Services.Interfaces
{
    public interface IAuthService
    {
        AuthResponseDto Register(RegisterDto registerDto);
        AuthResponseDto Login(LoginDto loginDto);
        bool RevokeRefreshToken(string refreshToken);
        string RefreshAccessToken(string refreshToken);
    }
}


