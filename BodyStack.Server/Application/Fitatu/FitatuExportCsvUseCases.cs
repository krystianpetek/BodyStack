using System.Globalization;
using System.Text;

namespace BodyStack.Server.Application.Fitatu;

internal static class FitatuCsvBuilder
{
    internal sealed record CsvRow(
        string Date,
        decimal Energy,
        decimal Protein,
        decimal Fat,
        decimal Carbohydrate,
        decimal Fiber,
        decimal Sugars,
        decimal Salt);

    internal static string BuildCsv(IEnumerable<CsvRow> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("date,kcal,protein,fat,carbohydrate,fiber,sugars,salt");

        static string F(decimal value)
            => decimal.Round(value, 1, MidpointRounding.AwayFromZero).ToString("0.0", CultureInfo.InvariantCulture);

        foreach (var r in rows)
        {
            sb.Append(r.Date);
            sb.Append(',');
            sb.Append(F(r.Energy));
            sb.Append(',');
            sb.Append(F(r.Protein));
            sb.Append(',');
            sb.Append(F(r.Fat));
            sb.Append(',');
            sb.Append(F(r.Carbohydrate));
            sb.Append(',');
            sb.Append(F(r.Fiber));
            sb.Append(',');
            sb.Append(F(r.Sugars));
            sb.Append(',');
            sb.AppendLine(F(r.Salt));
        }

        return sb.ToString();
    }
}

public sealed class FitatuExportDayCsvUseCase
{
    private readonly IFitatuSessionRepository _sessions;
    private readonly IMonthDaySummaryRepository _summaries;
    private readonly FitatuGetDayUseCase _getDay;

    public FitatuExportDayCsvUseCase(
        IFitatuSessionRepository sessions,
        IMonthDaySummaryRepository summaries,
        FitatuGetDayUseCase getDay)
    {
        _sessions = sessions;
        _summaries = summaries;
        _getDay = getDay;
    }

    public async Task<string> ExecuteAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetLatestAsync(cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException("Fitatu session not found. Please login first.");
        }

        var dateString = date.ToString("yyyy-MM-dd");
        var existing = await _summaries.GetByDateAsync(session.FitatuUserId, dateString, cancellationToken);

        if (existing is not null && string.Equals(existing.Status, MonthDaySummaryStatus.Ready, StringComparison.Ordinal))
        {
            return FitatuCsvBuilder.BuildCsv(new[]
            {
                new FitatuCsvBuilder.CsvRow(existing.Date, existing.Energy, existing.Protein, existing.Fat, existing.Carbohydrate, existing.Fiber, existing.Sugars, existing.Salt)
            });
        }

        var computed = await _getDay.ExecuteAsync(date, cancellationToken);

        return FitatuCsvBuilder.BuildCsv(new[]
        {
            new FitatuCsvBuilder.CsvRow(dateString, computed.Totals.Energy, computed.Totals.Protein, computed.Totals.Fat, computed.Totals.Carbohydrate, computed.Totals.Fiber, computed.Totals.Sugars, computed.Totals.Salt)
        });
    }
}

public sealed class FitatuExportMonthCsvUseCase
{
    private readonly IFitatuSessionRepository _sessions;
    private readonly IMonthDaySummaryRepository _summaries;

    public FitatuExportMonthCsvUseCase(IFitatuSessionRepository sessions, IMonthDaySummaryRepository summaries)
    {
        _sessions = sessions;
        _summaries = summaries;
    }

    public async Task<string> ExecuteAsync(string yearMonth, CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetLatestAsync(cancellationToken);
        if (session is null)
        {
            throw new InvalidOperationException("Fitatu session not found. Please login first.");
        }

        if (!TryParseYearMonth(yearMonth, out var year, out var month))
        {
            throw new ArgumentException("Invalid YearMonth format. Expected YYYY-MM.", nameof(yearMonth));
        }

        var rows = await _summaries.GetByYearMonthAsync(session.FitatuUserId, yearMonth, cancellationToken);

        var expectedDates = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
            .Select(d => new DateOnly(year, month, d).ToString("yyyy-MM-dd"))
            .ToArray();

        var ready = rows
            .Where(r => string.Equals(r.Status, MonthDaySummaryStatus.Ready, StringComparison.Ordinal))
            .ToDictionary(r => r.Date, r => r);

        var missing = expectedDates.Where(d => !ready.ContainsKey(d)).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidOperationException($"Month export requires computed days. Missing: {string.Join(',', missing)}");
        }

        var csvRows = expectedDates
            .Select(d => ready[d])
            .Select(r => new FitatuCsvBuilder.CsvRow(r.Date, r.Energy, r.Protein, r.Fat, r.Carbohydrate, r.Fiber, r.Sugars, r.Salt));

        return FitatuCsvBuilder.BuildCsv(csvRows);
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
