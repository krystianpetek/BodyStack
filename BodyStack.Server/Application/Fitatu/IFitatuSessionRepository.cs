namespace BodyStack.Server.Application.Fitatu;

public interface IFitatuSessionRepository
{
    Task UpsertAsync(string fitatuUserId, string token, string refreshToken, CancellationToken cancellationToken = default);

    Task<FitatuSessionDto?> GetByFitatuUserIdAsync(string fitatuUserId, CancellationToken cancellationToken = default);

    Task<FitatuSessionDto?> GetLatestAsync(CancellationToken cancellationToken = default);
}

public sealed record FitatuSessionDto(string FitatuUserId, string Token, string RefreshToken, DateTimeOffset UpdatedAt);
