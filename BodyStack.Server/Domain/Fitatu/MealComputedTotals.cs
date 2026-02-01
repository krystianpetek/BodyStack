namespace BodyStack.Server.Domain.Fitatu;

public sealed record MealComputedTotals(
    string MealKey,
    string MealName,
    string? MealTime,
    DayComputedTotals Totals);
