using System.Net.Http.Json;
using ECommerce.Huit.API;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ECommerce.Huit.IntegrationTests;

public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Override JWT settings for testing
                services.PostConfigure<Microsoft.Extensions.Configuration.Jwt>(c =>
                {
                    c.Key = "test_key_for_integration_tests_32chars_minimum";
                    c.Issuer = "TestIssuer";
                    c.Audience = "TestAudience";
                });
            });
        });
    }

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerDto = new
        {
            full_name = "Test User",
            email = "test@example.com",
            phone = "0909123456",
            password = "Password123"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        // Arrange
        var client = _factory.CreateClient();
        var registerDto = new
        {
            full_name = "Test User 1",
            email = "duplicate@example.com",
            phone = "0909123456",
            password = "Password123"
        };

        // First registration
        await client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Act - Second registration with same email
        var response = await client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
}
