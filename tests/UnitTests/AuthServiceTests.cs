using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Auth;
using ECommerce.Huit.Application.Services;
using ECommerce.Huit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ECommerce.Huit.UnitTests;

public class AuthServiceTests
{
    private Mock<ApplicationDbContext> CreateMockContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        return Mock.Of<ApplicationDbContext>(c => c.Users == context.Users && c.SaveChangesAsync(It.IsAny<CancellationToken>()) == Task.FromResult(1));
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenEmailIsUnique()
    {
        // Arrange
        var mockContext = CreateMockContext();
        var mockTokenGenerator = new Mock<IJwtTokenGenerator>();
        mockTokenGenerator.Setup(t => t.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("test_access_token");
        mockTokenGenerator.Setup(t => t.GenerateRefreshToken())
            .Returns("test_refresh_token");

        var authService = new AuthService(mockContext, mockTokenGenerator.Object);
        var registerDto = new RegisterDto
        {
            FullName = "Test User",
            Email = "test@example.com",
            Phone = "0909123456",
            Password = "Password123"
        };

        // Act
        var result = await authService.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(registerDto.Email, result.Email);
        Assert.Equal(registerDto.FullName, result.FullName);
        Assert.Equal("CUSTOMER", result.Role);
        Assert.Equal("test_access_token", result.AccessToken);
        Assert.Equal("test_refresh_token", result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUser_WhenCredentialsAreValid()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        var hashedPassword = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Password123"));
        context.Users.Add(new User
        {
            FullName = "Test User",
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            Role = Domain.Enums.UserRole.CUSTOMER,
            Status = Domain.Enums.UserStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var mockTokenGenerator = new Mock<IJwtTokenGenerator>();
        mockTokenGenerator.Setup(t => t.GenerateAccessToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("test_access_token");
        mockTokenGenerator.Setup(t => t.GenerateRefreshToken())
            .Returns("test_refresh_token");

        var authService = new AuthService(context, mockTokenGenerator.Object);
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("test_access_token", result.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var mockContext = CreateMockContext();
        var mockTokenGenerator = new Mock<IJwtTokenGenerator>();
        var authService = new AuthService(mockContext, mockTokenGenerator.Object);
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123"
        };

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        Assert.Null(result);
    }
}
