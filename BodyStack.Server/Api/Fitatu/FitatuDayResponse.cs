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
    private static decimal R(decimal v) => decimal.Round(v, 1, MidpointRounding.AwayFromZero);

    public static FitatuTotals From(DayComputedTotals totals)
        => new(R(totals.Energy), R(totals.Protein), R(totals.Fat), R(totals.Carbohydrate), R(totals.Fiber), R(totals.Sugars), R(totals.Salt));
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
