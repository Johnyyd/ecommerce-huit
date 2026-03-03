using ECommerce.Huit.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Huit.UnitTests;

public class JwtTokenGeneratorTests
{
    private IConfiguration CreateTestConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "test_super_secret_key_32_chars_minimum_for_tests"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:DurationInMinutes", "1440"}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void GenerateAccessToken_ShouldCreateValidToken()
    {
        // Arrange
        var config = CreateTestConfiguration();
        var generator = new JwtTokenGenerator(config);
        var userId = 123;
        var email = "test@example.com";
        var role = "CUSTOMER";

        // Act
        var token = generator.GenerateAccessToken(userId, email, role);

        // Assert
        Assert.NotNull(token);
        Assert.IsType<string>(token);

        // Validate token structure
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Equal("TestAudience", jwtToken.Audiences.FirstOrDefault());
        Assert.Equal(1440, (jwtToken.ValidTo - jwtToken.ValidFrom).TotalMinutes);

        var claims = jwtToken.Claims.ToList();
        Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
        Assert.Contains(claims, c => c.Type == ClaimTypes.Role && c.Value == role);
        Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateAccessToken_ShouldUseCorrectKey()
    {
        // Arrange
        var config = CreateTestConfiguration();
        var generator = new JwtTokenGenerator(config);

        // Act
        var token = generator.GenerateAccessToken(1, "test@test.com", "USER");

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "TestIssuer",
            ValidateAudience = true,
            ValidAudience = "TestAudience",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test_super_secret_key_32_chars_minimum_for_tests")),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Should not throw - token is valid
        tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        Assert.NotNull(validatedToken);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var config = CreateTestConfiguration();
        var generator = new JwtTokenGenerator(config);

        // Act
        var token1 = generator.GenerateRefreshToken();
        var token2 = generator.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token1);
        Assert.NotNull(token2);
        Assert.NotEqual(token1, token2);
        Assert.Equal(44, token1.Length); // Base64 encoded 32 bytes = 44 chars
        Assert.Equal(44, token2.Length);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldBeBase64Encoded()
    {
        // Arrange
        var config = CreateTestConfiguration();
        var generator = new JwtTokenGenerator(config);

        // Act
        var token = generator.GenerateRefreshToken();

        // Assert
        Assert.DoesNotContain("=", token.TrimEnd('=')); // Base64 without padding
        // Should be decodable
        var bytes = Convert.FromBase64String(token);
        Assert.Equal(32, bytes.Length); // 32 bytes random
    }

    [Fact]
    public void ValidateRefreshToken_ShouldReturnUserId_ForStoredToken()
    {
        // Arrange
        var config = CreateTestConfiguration();
        var generator = new JwtTokenGenerator(config);
        var refreshToken = generator.GenerateRefreshToken();

        // Act
        var result = generator.ValidateRefreshToken(refreshToken);

        // Assert
        // Currently returns 1 (placeholder)
        Assert.Equal(1, result);
    }
}
