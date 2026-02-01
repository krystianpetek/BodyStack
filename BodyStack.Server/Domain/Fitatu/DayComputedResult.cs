namespace BodyStack.Server.Domain.Fitatu;

using System.Collections.Generic;

public sealed record DayComputedResult(
    DayComputedTotals Totals,
    IReadOnlyList<BodyStack.Server.Domain.Fitatu.MealComputedTotals> Meals);
