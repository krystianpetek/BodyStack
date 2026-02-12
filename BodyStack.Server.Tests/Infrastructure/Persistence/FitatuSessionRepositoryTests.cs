using BodyStack.Server.Application.Fitatu;
using BodyStack.Server.Infrastructure.Persistence;
using BodyStack.Server.Infrastructure.Persistence.Entities;
using BodyStack.Server.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BodyStack.Server.Tests.Infrastructure.Persistence;

public class FitatuSessionRepositoryTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly FitatuSessionRepository _repository;

    public FitatuSessionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _repository = new FitatuSessionRepository(_db, new TestTokenProtector(), NullLogger<FitatuSessionRepository>.Instance);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task GetLatestAsync_With_Empty_Database_Returns_Null()
    {
        // Act
        var result = await _repository.GetLatestAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestAsync_With_One_Session_Returns_That_Session()
    {
        // Arrange
        var session = new FitatuSession
        {
            Id = Guid.NewGuid(),
            FitatuUserId = "user1",
            TokenProtected = "protected_token_1",
            RefreshTokenProtected = "protected_refresh_1",
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.FitatuSessions.Add(session);
        await _db.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user1", result.FitatuUserId);
    }

    [Fact]
    public async Task GetLatestAsync_With_Multiple_Sessions_Returns_Most_Recent()
    {
        // Arrange
        var olderSession = new FitatuSession
        {
            Id = Guid.NewGuid(),
            FitatuUserId = "user1",
            TokenProtected = "protected_token_1",
            RefreshTokenProtected = "protected_refresh_1",
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-2)
        };

        var newerSession = new FitatuSession
        {
            Id = Guid.NewGuid(),
            FitatuUserId = "user2",
            TokenProtected = "protected_token_2",
            RefreshTokenProtected = "protected_refresh_2",
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        var newestSession = new FitatuSession
        {
            Id = Guid.NewGuid(),
            FitatuUserId = "user3",
            TokenProtected = "protected_token_3",
            RefreshTokenProtected = "protected_refresh_3",
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.FitatuSessions.AddRange(olderSession, newerSession, newestSession);
        await _db.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user3", result.FitatuUserId);
    }

    [Fact]
    public async Task UpsertAsync_Creates_New_Session_When_Not_Exists()
    {
        // Arrange
        var fitatuUserId = "newuser";
        var token = "my_token";
        var refreshToken = "my_refresh_token";

        // Act
        await _repository.UpsertAsync(fitatuUserId, token, refreshToken);

        // Assert
        var savedSession = await _db.FitatuSessions.FirstOrDefaultAsync(s => s.FitatuUserId == fitatuUserId);
        Assert.NotNull(savedSession);
        Assert.Equal(fitatuUserId, savedSession.FitatuUserId);
    }

    [Fact]
    public async Task UpsertAsync_Updates_Existing_Session()
    {
        // Arrange
        var fitatuUserId = "existinguser";
        var oldToken = "old_token";
        var oldRefreshToken = "old_refresh_token";

        await _repository.UpsertAsync(fitatuUserId, oldToken, oldRefreshToken);

        var newToken = "new_token";
        var newRefreshToken = "new_refresh_token";

        // Act
        await _repository.UpsertAsync(fitatuUserId, newToken, newRefreshToken);

        // Assert
        var sessions = await _db.FitatuSessions.Where(s => s.FitatuUserId == fitatuUserId).ToListAsync();
        Assert.Single(sessions);
        Assert.Equal("protected_new_token", sessions[0].TokenProtected);
    }

    [Fact]
    public async Task GetByFitatuUserIdAsync_Returns_Null_When_Not_Found()
    {
        // Act
        var result = await _repository.GetByFitatuUserIdAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByFitatuUserIdAsync_Returns_Session_When_Found()
    {
        // Arrange
        var session = new FitatuSession
        {
            Id = Guid.NewGuid(),
            FitatuUserId = "targetuser",
            TokenProtected = "protected_token",
            RefreshTokenProtected = "protected_refresh",
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _db.FitatuSessions.Add(session);
        await _db.SaveChangesAsync();

        // Act
        var result = await _repository.GetByFitatuUserIdAsync("targetuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("targetuser", result.FitatuUserId);
    }

    [Fact]
    public async Task ClearAsync_Removes_All_Sessions()
    {
        // Arrange
        _db.FitatuSessions.Add(new FitatuSession
        {
            Id = Guid.NewGuid(),
            FitatuUserId = "user1",
            TokenProtected = "token1",
            RefreshTokenProtected = "refresh1",
            UpdatedAt = DateTimeOffset.UtcNow
        });
        _db.FitatuSessions.Add(new FitatuSession
        {
            Id = Guid.NewGuid(),
            FitatuUserId = "user2",
            TokenProtected = "token2",
            RefreshTokenProtected = "refresh2",
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync();

        // Act
        await _repository.ClearAsync();

        // Assert
        var count = await _db.FitatuSessions.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task UpsertAsync_With_Empty_FitatuUserId_Throws_ArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.UpsertAsync("", "token", "refresh"));
    }

    [Fact]
    public async Task UpsertAsync_With_Empty_Token_Throws_ArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.UpsertAsync("user", "", "refresh"));
    }

    // Helper test double for ITokenProtector
    private class TestTokenProtector : ITokenProtector
    {
        public string Protect(string plainText)
        {
            return $"protected_{plainText}";
        }

        public string Unprotect(string protectedText)
        {
            return protectedText?.Replace("protected_", "") ?? string.Empty;
        }
    }
}
