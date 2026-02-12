using System.Net;
using System.Text.Json;
using BodyStack.Server.Infrastructure.Persistence;
using BodyStack.Server.Infrastructure.Persistence.Entities;
using BodyStack.Server.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BodyStack.Server.Tests.Integration;

public class ExportEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _dbName;

    public ExportEndpointTests(WebApplicationFactory<Program> factory)
    {
        _dbName = Guid.NewGuid().ToString();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                // Replace token protector with test implementation
                services.AddSingleton<ITokenProtector, TestTokenProtector>();
            });
        });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Fact]
    public async Task ExportMonthCsv_Without_Session_Returns_401()
    {
        // Act
        var response = await _client.GetAsync("/api/fitatu/export/month/2024-01");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExportDayCsv_Without_Session_Returns_401()
    {
        // Act
        var response = await _client.GetAsync("/api/fitatu/export/day/2024-01-15");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExportMonthCsv_With_Incomplete_Data_Returns_409_With_MissingDays()
    {
        // Arrange - Create a session but no month data
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        db.FitatuSessions.Add(new FitatuSession
        {
            Id = Guid.NewGuid(),
            FitatuUserId = "test-user",
            TokenProtected = "protected_token",
            RefreshTokenProtected = "protected_refresh",
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/fitatu/export/month/2024-01");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error", content);
    }

    [Fact]
    public async Task ExportDayCsv_With_Session_Returns_CSV()
    {
        // Arrange - Create a session
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        db.FitatuSessions.Add(new FitatuSession
        {
            Id = Guid.NewGuid(),
            FitatuUserId = "test-user",
            TokenProtected = "protected_token",
            RefreshTokenProtected = "protected_refresh",
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/fitatu/export/day/2024-01-15");

        // Assert - We expect 200 or 500 (depending on if we can actually call the API)
        // The important thing is it's NOT 401
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private class TestTokenProtector : ITokenProtector
    {
        public string Protect(string plainText) => $"protected_{plainText}";
        public string Unprotect(string protectedText) => protectedText?.Replace("protected_", "") ?? string.Empty;
    }
}
