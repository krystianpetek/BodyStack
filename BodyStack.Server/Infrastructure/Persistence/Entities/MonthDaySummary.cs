namespace BodyStack.Server.Infrastructure.Persistence.Entities;

public sealed class MonthDaySummary
{
    public Guid Id { get; set; }
    public required string FitatuUserId { get; set; }
    public required string YearMonth { get; set; }
    public required string Date { get; set; }

    public decimal Energy { get; set; }
    public decimal Protein { get; set; }
    public decimal Fat { get; set; }
    public decimal Carbohydrate { get; set; }
    public decimal Fiber { get; set; }
    public decimal Sugars { get; set; }
    public decimal Salt { get; set; }

    public required string Status { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
