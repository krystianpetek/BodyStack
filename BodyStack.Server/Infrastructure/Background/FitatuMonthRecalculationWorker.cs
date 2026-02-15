using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using BodyStack.Server.Application.Fitatu;
using BodyStack.Server.Domain.Fitatu;
using BodyStack.Server.Infrastructure.Persistence;
using BodyStack.Server.Infrastructure.Persistence.Entities;
using BodyStack.Server.Integrations.Fitatu;
using BodyStack.Server.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BodyStack.Server.Infrastructure.Background;

public sealed class FitatuMonthRecalculationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBackgroundTaskQueue<FitatuMonthRecalculationRequest> _queue;
    private readonly IHubContext<FitatuMonthHub> _hub;

    public FitatuMonthRecalculationWorker(
        IServiceScopeFactory scopeFactory,
        IBackgroundTaskQueue<FitatuMonthRecalculationRequest> queue,
        IHubContext<FitatuMonthHub> hub)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = await _queue.DequeueAsync(stoppingToken);
            await ProcessAsync(request, stoppingToken);
        }
    }

    private async Task ProcessAsync(FitatuMonthRecalculationRequest request, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var sessions = scope.ServiceProvider.GetRequiredService<IFitatuSessionRepository>();
        var fitatuClient = scope.ServiceProvider.GetRequiredService<IFitatuClient>();
        var calculator = scope.ServiceProvider.GetRequiredService<FitatuDayPlanTotalsCalculator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var session = await sessions.GetByFitatuUserIdAsync(request.FitatuUserId, cancellationToken);
        if (session is null)
        {
            await _hub.Clients.Group($"user-{request.FitatuUserId}").SendAsync("Progress", new { done = 0, total = 0, error = "Fitatu session not found. Please login first." }, cancellationToken);
            return;
        }

        if (!TryParseYearMonth(request.YearMonth, out var year, out var month))
        {
            await _hub.Clients.Group($"user-{request.FitatuUserId}").SendAsync("Progress", new { done = 0, total = 0, error = "Invalid YearMonth format. Expected YYYY-MM." }, cancellationToken);
            return;
        }

        var totalDays = DateTime.DaysInMonth(year, month);
        var done = 0;

        for (var day = 1; day <= totalDays; day++)
        {
            var dateOnly = new DateOnly(year, month, day);
            var date = dateOnly.ToString("yyyy-MM-dd");

            await UpsertStatusAsync(db, session.FitatuUserId, request.YearMonth, date, MonthDaySummaryStatus.Pending, null, cancellationToken);

            try
            {
                using var json = await fitatuClient.GetDietAndActivityPlanDayAsync(session.FitatuUserId, dateOnly, session.Token, cancellationToken)
                .FirstAsync()
                .ToTask(cancellationToken);

                var computed = calculator.Compute(json);

                await UpsertReadyAsync(db, session.FitatuUserId, request.YearMonth, date, computed.Totals, cancellationToken);

                done++;
                await _hub.Clients.Group($"user-{request.FitatuUserId}").SendAsync("DayReady", new { date }, cancellationToken);
                await _hub.Clients.Group($"user-{request.FitatuUserId}").SendAsync("Progress", new { done, total = totalDays }, cancellationToken);
            }
            catch (Exception ex)
            {
                await UpsertStatusAsync(db, session.FitatuUserId, request.YearMonth, date, MonthDaySummaryStatus.Error, ex.Message, cancellationToken);

                done++;
                await _hub.Clients.Group($"user-{request.FitatuUserId}").SendAsync("Progress", new { done, total = totalDays }, cancellationToken);
            }
        }
    }

    private static async Task UpsertReadyAsync(
        AppDbContext db,
        string fitatuUserId,
        string yearMonth,
        string date,
        DayComputedTotals totals,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = await db.MonthDaySummaries
            .SingleOrDefaultAsync(x => x.FitatuUserId == fitatuUserId && x.Date == date, cancellationToken);

        if (entity is null)
        {
            db.MonthDaySummaries.Add(new MonthDaySummary
            {
                Id = Guid.NewGuid(),
                FitatuUserId = fitatuUserId,
                YearMonth = yearMonth,
                Date = date,
                Energy = totals.Energy,
                Protein = totals.Protein,
                Fat = totals.Fat,
                Carbohydrate = totals.Carbohydrate,
                Fiber = totals.Fiber,
                Sugars = totals.Sugars,
                Salt = totals.Salt,
                Status = MonthDaySummaryStatus.Ready,
                ErrorMessage = null,
                UpdatedAt = now,
            });
        }
        else
        {
            entity.YearMonth = yearMonth;
            entity.Energy = totals.Energy;
            entity.Protein = totals.Protein;
            entity.Fat = totals.Fat;
            entity.Carbohydrate = totals.Carbohydrate;
            entity.Fiber = totals.Fiber;
            entity.Sugars = totals.Sugars;
            entity.Salt = totals.Salt;
            entity.Status = MonthDaySummaryStatus.Ready;
            entity.ErrorMessage = null;
            entity.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task UpsertStatusAsync(
        AppDbContext db,
        string fitatuUserId,
        string yearMonth,
        string date,
        string status,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = await db.MonthDaySummaries
            .SingleOrDefaultAsync(x => x.FitatuUserId == fitatuUserId && x.Date == date, cancellationToken);

        if (entity is null)
        {
            db.MonthDaySummaries.Add(new MonthDaySummary
            {
                Id = Guid.NewGuid(),
                FitatuUserId = fitatuUserId,
                YearMonth = yearMonth,
                Date = date,
                Energy = 0,
                Protein = 0,
                Fat = 0,
                Carbohydrate = 0,
                Fiber = 0,
                Sugars = 0,
                Salt = 0,
                Status = status,
                ErrorMessage = errorMessage,
                UpdatedAt = now,
            });
        }
        else
        {
            entity.YearMonth = yearMonth;
            entity.Status = status;
            entity.ErrorMessage = errorMessage;
            entity.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static bool TryParseYearMonth(string yearMonth, out int year, out int month)
    {
        year = 0;
        month = 0;

        var parts = yearMonth.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        return int.TryParse(parts[0], out year) && int.TryParse(parts[1], out month) && month is >= 1 and <= 12;
    }
}
