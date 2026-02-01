using BodyStack.Server.Domain.Fitatu;
using BodyStack.Server.Integrations.Fitatu;

namespace BodyStack.Server.Application.Fitatu;

public sealed class FitatuGetDayUseCase
{
    private readonly IFitatuSessionRepository _sessions;
    private readonly IFitatuClient _fitatuClient;
    private readonly FitatuDayPlanTotalsCalculator _calculator;

    public FitatuGetDayUseCase(
        IFitatuSessionRepository sessions,
        IFitatuClient fitatuClient,
        FitatuDayPlanTotalsCalculator calculator)
    {
        _sessions = sessions;
        _fitatuClient = fitatuClient;
        _calculator = calculator;
    }

    public async Task<DayComputedResult> ExecuteAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetLatestAsync(cancellationToken);

        if (session is null)
        {
            throw new InvalidOperationException("Fitatu session not found. Please login first.");
        }

        using var dayJson = await _fitatuClient.GetDietAndActivityPlanDayAsync(session.FitatuUserId, date, session.Token, cancellationToken);
        return _calculator.Compute(dayJson);
    }
}
