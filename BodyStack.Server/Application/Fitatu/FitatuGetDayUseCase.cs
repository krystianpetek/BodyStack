using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using BodyStack.Server.Domain.Exceptions;
using BodyStack.Server.Domain.Fitatu;
using BodyStack.Server.Integrations.Fitatu;

namespace BodyStack.Server.Application.Fitatu;

/// <summary>
/// Use case for retrieving a single day's data from Fitatu with streaming support
/// </summary>
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

    /// <summary>
    /// Executes the use case and returns result as a reactive observable
    /// </summary>
    public IObservable<DayComputedResult> Execute(DateOnly date, CancellationToken cancellationToken = default)
    {
        return Observable.Create<DayComputedResult>(async (observer, ct) =>
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ct);
            
            try
            {
                var session = await _sessions.GetLatestAsync(linkedCts.Token);

                if (session is null)
                {
                    throw new FitatuSessionNotFoundException(null);
                }

                using var subscription = _fitatuClient.GetDietAndActivityPlanDayAsync(session.FitatuUserId, date, session.Token, linkedCts.Token)
                    .Select(json => _calculator.Compute(json))
                    .Subscribe(
                        result => observer.OnNext(result),
                        error => observer.OnError(error),
                        () => observer.OnCompleted());
                
                // Wait for completion or cancellation
                try
                {
                    await Task.Delay(Timeout.Infinite, linkedCts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelled or completed
                }
            }
            catch (OperationCanceledException)
            {
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
            finally
            {
                linkedCts.Dispose();
            }

            return Disposable.Empty;
        });
    }

    /// <summary>
    /// Executes the use case asynchronously (legacy compatibility)
    /// </summary>
    public async Task<DayComputedResult> ExecuteAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await Execute(date, cancellationToken).FirstAsync().ToTask(cancellationToken);
    }
}
