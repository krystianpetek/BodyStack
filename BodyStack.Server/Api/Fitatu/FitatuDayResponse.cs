using BodyStack.Server.Domain.Fitatu;

namespace BodyStack.Server.Api.Fitatu;

public sealed record FitatuDayResponse(
    string Date,
    FitatuTotals Totals,
    IReadOnlyList<FitatuMealTotals> Meals);

public sealed record FitatuTotals(
    decimal Energy,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrate,
    decimal Fiber,
    decimal Sugars,
    decimal Salt)
{
    public static FitatuTotals From(DayComputedTotals totals)
        => new(totals.Energy, totals.Protein, totals.Fat, totals.Carbohydrate, totals.Fiber, totals.Sugars, totals.Salt);
}

public sealed record FitatuMealTotals(
    string MealKey,
    string MealName,
    string? MealTime,
    FitatuTotals Totals)
{
    public static FitatuMealTotals From(MealComputedTotals meal)
        => new(meal.MealKey, meal.MealName, meal.MealTime, FitatuTotals.From(meal.Totals));
}
