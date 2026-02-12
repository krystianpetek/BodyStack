using BodyStack.Server.Application.Fitatu;
using BodyStack.Server.Infrastructure.Persistence.Entities;
using BodyStack.Server.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

namespace BodyStack.Server.Infrastructure.Persistence;

public sealed class FitatuSessionRepository : IFitatuSessionRepository
{
    private readonly AppDbContext _db;
    private readonly ITokenProtector _tokenProtector;
    private readonly ILogger<FitatuSessionRepository> _logger;
    private const int SlowQueryThresholdMs = 100;

    public FitatuSessionRepository(AppDbContext db, ITokenProtector tokenProtector, ILogger<FitatuSessionRepository> logger)
    {
        _db = db;
        _tokenProtector = tokenProtector;
        _logger = logger;
    }

    private void LogSlowQuery(string operationName, long elapsedMs)
    {
        if (elapsedMs > SlowQueryThresholdMs)
        {
            _logger.LogWarning("Slow query detected in {OperationName}: {ElapsedMs}ms (threshold: {ThresholdMs}ms)",
                operationName, elapsedMs, SlowQueryThresholdMs);
        }
    }

    public async Task UpsertAsync(string fitatuUserId, string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fitatuUserId))
        {
            throw new ArgumentException("FitatuUserId is required.", nameof(fitatuUserId));
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token is required.", nameof(token));
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("RefreshToken is required.", nameof(refreshToken));
        }

        var now = DateTimeOffset.UtcNow;
        var existing = await _db.FitatuSessions.SingleOrDefaultAsync(x => x.FitatuUserId == fitatuUserId, cancellationToken);

        if (existing is null)
        {
            _db.FitatuSessions.Add(new FitatuSession
            {
                Id = Guid.NewGuid(),
                FitatuUserId = fitatuUserId,
                TokenProtected = _tokenProtector.Protect(token),
                RefreshTokenProtected = _tokenProtector.Protect(refreshToken),
                UpdatedAt = now,
            });
        }
        else
        {
            existing.TokenProtected = _tokenProtector.Protect(token);
            existing.RefreshTokenProtected = _tokenProtector.Protect(refreshToken);
            existing.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<FitatuSessionDto?> GetByFitatuUserIdAsync(string fitatuUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fitatuUserId))
        {
            throw new ArgumentException("FitatuUserId is required.", nameof(fitatuUserId));
        }

        var stopwatch = Stopwatch.StartNew();

        var entity = await _db.FitatuSessions.SingleOrDefaultAsync(x => x.FitatuUserId == fitatuUserId, cancellationToken);

        stopwatch.Stop();
        LogSlowQuery(nameof(GetByFitatuUserIdAsync), stopwatch.ElapsedMilliseconds);

        if (entity is null)
        {
            return null;
        }

        return new FitatuSessionDto(
            entity.FitatuUserId,
            _tokenProtector.Unprotect(entity.TokenProtected),
            _tokenProtector.Unprotect(entity.RefreshTokenProtected),
            entity.UpdatedAt);
    }

    public async Task<FitatuSessionDto?> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var entity = await _db.FitatuSessions
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        stopwatch.Stop();
        LogSlowQuery(nameof(GetLatestAsync), stopwatch.ElapsedMilliseconds);

        if (entity is null)
        {
            return null;
        }

        return new FitatuSessionDto(
            entity.FitatuUserId,
            _tokenProtector.Unprotect(entity.TokenProtected),
            _tokenProtector.Unprotect(entity.RefreshTokenProtected),
            entity.UpdatedAt);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _db.FitatuSessions.RemoveRange(_db.FitatuSessions);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
