using BodyStack.Server.Infrastructure.Background;

namespace BodyStack.Server.Application.Fitatu;

public sealed class FitatuStartMonthRecalculationUseCase
{
    private readonly IFitatuSessionRepository _sessions;
    private readonly IBackgroundTaskQueue<FitatuMonthRecalculationRequest> _queue;

    public FitatuStartMonthRecalculationUseCase(
        IFitatuSessionRepository sessions,
        IBackgroundTaskQueue<FitatuMonthRecalculationRequest> queue)
    {
        _sessions = sessions;
        _queue = queue;
    }

    public async Task ExecuteAsync(string yearMonth, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(yearMonth))
        {
            throw new ArgumentException("YearMonth is required.", nameof(yearMonth));
        }

        var session = await _sessions.GetLatestAsync(cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException("Fitatu session not found. Please login first.");
        }

        await _queue.QueueAsync(new FitatuMonthRecalculationRequest(session.FitatuUserId, yearMonth), cancellationToken);
    }
}
