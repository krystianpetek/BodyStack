using BodyStack.Server.Application.Fitatu;
using BodyStack.Server.Infrastructure.Persistence.Entities;
using BodyStack.Server.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace BodyStack.Server.Infrastructure.Persistence;

public sealed class FitatuSessionRepository : IFitatuSessionRepository
{
    private readonly AppDbContext _db;
    private readonly ITokenProtector _tokenProtector;

    public FitatuSessionRepository(AppDbContext db, ITokenProtector tokenProtector)
    {
        _db = db;
        _tokenProtector = tokenProtector;
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

        var entity = await _db.FitatuSessions.SingleOrDefaultAsync(x => x.FitatuUserId == fitatuUserId, cancellationToken);

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
        var entity = await _db.FitatuSessions
            .OrderByDescending(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

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
}
