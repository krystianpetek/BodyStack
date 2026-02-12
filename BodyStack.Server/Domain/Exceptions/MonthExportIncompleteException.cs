namespace BodyStack.Server.Domain.Exceptions;

public class MonthExportIncompleteException : DomainException
{
    public int Year { get; }
    public int Month { get; }
    public IReadOnlyList<string> MissingDays { get; }

    public MonthExportIncompleteException(int year, int month, IEnumerable<string> missingDays)
        : base("MONTH_EXPORT_INCOMPLETE", 
               $"Month export incomplete for {year}-{month:D2}. Missing days: {string.Join(", ", missingDays)}")
    {
        Year = year;
        Month = month;
        MissingDays = missingDays.ToList().AsReadOnly();
    }
}
