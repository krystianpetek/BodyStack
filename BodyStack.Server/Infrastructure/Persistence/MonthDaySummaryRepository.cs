using BodyStack.Server.Application.Fitatu;
using Microsoft.EntityFrameworkCore;

namespace BodyStack.Server.Infrastructure.Persistence;

public sealed class MonthDaySummaryRepository : IMonthDaySummaryRepository
{
    private readonly AppDbContext _db;

    public MonthDaySummaryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MonthDaySummaryDto?> GetByDateAsync(string fitatuUserId, string date, CancellationToken cancellationToken = default)
    {
        var entity = await _db.MonthDaySummaries
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.FitatuUserId == fitatuUserId && x.Date == date, cancellationToken);

        return entity is null
            ? null
            : new MonthDaySummaryDto(
                entity.FitatuUserId,
                entity.YearMonth,
                entity.Date,
                entity.Energy,
                entity.Protein,
                entity.Fat,
                entity.Carbohydrate,
                entity.Fiber,
                entity.Sugars,
                entity.Salt,
                entity.Status,
                entity.ErrorMessage,
                entity.UpdatedAt);
    }

    public async Task<IReadOnlyList<MonthDaySummaryDto>> GetByYearMonthAsync(string fitatuUserId, string yearMonth, CancellationToken cancellationToken = default)
    {
        return await _db.MonthDaySummaries
            .AsNoTracking()
            .Where(x => x.FitatuUserId == fitatuUserId && x.YearMonth == yearMonth)
            .OrderBy(x => x.Date)
            .Select(x => new MonthDaySummaryDto(
                x.FitatuUserId,
                x.YearMonth,
                x.Date,
                x.Energy,
                x.Protein,
                x.Fat,
                x.Carbohydrate,
                x.Fiber,
                x.Sugars,
                x.Salt,
                x.Status,
                x.ErrorMessage,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
