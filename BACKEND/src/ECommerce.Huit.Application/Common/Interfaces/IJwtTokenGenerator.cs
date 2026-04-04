using ECommerce.Huit.Domain.Entities;

namespace ECommerce.Huit.Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
    }
}
