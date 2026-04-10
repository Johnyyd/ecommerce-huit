namespace HuitShopDB.Services.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(string userId, string email, string role);
        string GenerateRefreshToken();
    }
}

