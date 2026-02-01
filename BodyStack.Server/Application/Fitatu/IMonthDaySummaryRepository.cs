namespace BodyStack.Server.Application.Fitatu;

public interface IMonthDaySummaryRepository
{
    Task<MonthDaySummaryDto?> GetByDateAsync(string fitatuUserId, string date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthDaySummaryDto>> GetByYearMonthAsync(string fitatuUserId, string yearMonth, CancellationToken cancellationToken = default);
}

public sealed record MonthDaySummaryDto(
    string FitatuUserId,
    string YearMonth,
    string Date,
    decimal Energy,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrate,
    decimal Fiber,
    decimal Sugars,
    decimal Salt,
    string Status,
    string? ErrorMessage,
    DateTimeOffset UpdatedAt);
