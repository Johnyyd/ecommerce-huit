namespace ECommerce.Huit.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(int userId, string email, string role);
    string GenerateRefreshToken();
    int? ValidateRefreshToken(string refreshToken);
}
