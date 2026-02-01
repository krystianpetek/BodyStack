using System.Text.Json;

namespace BodyStack.Server.Domain.Fitatu;

public sealed class FitatuDayPlanTotalsCalculator
{
    private static readonly string[] DietPlanMealKeys =
    [
        "breakfast",
        "second_breakfast",
        "lunch",
        "dinner",
        "snack",
        "supper",
    ];

    public DayComputedResult Compute(JsonDocument dayPlanJson)
    {
        ArgumentNullException.ThrowIfNull(dayPlanJson);

        return Compute(dayPlanJson.RootElement);
    }

    public DayComputedResult Compute(JsonElement root)
    {
        if (!root.TryGetProperty("dietPlan", out var dietPlanElement) || dietPlanElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Fitatu day response does not contain object 'dietPlan'.");
        }

        var meals = new List<MealComputedTotals>();
        var dayTotals = DayComputedTotals.Zero;

        foreach (var mealKey in DietPlanMealKeys)
        {
            if (!dietPlanElement.TryGetProperty(mealKey, out var mealElement) || mealElement.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var mealName = GetString(mealElement, "mealName") ?? mealKey;
            var mealTime = GetString(mealElement, "mealTime");

            var mealTotals = DayComputedTotals.Zero;

            if (mealElement.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in itemsElement.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    mealTotals += new DayComputedTotals(
                        GetDecimal(item, "energy"),
                        GetDecimal(item, "protein"),
                        GetDecimal(item, "fat"),
                        GetDecimal(item, "carbohydrate"),
                        GetDecimal(item, "fiber"),
                        GetDecimal(item, "sugars"),
                        GetDecimal(item, "salt"));
                }
            }

            meals.Add(new MealComputedTotals(mealKey, mealName, mealTime, mealTotals));
            dayTotals += mealTotals;
        }

        return new DayComputedResult(dayTotals, meals);
    }

    private static decimal GetDecimal(JsonElement obj, string propertyName)
    {
        if (!obj.TryGetProperty(propertyName, out var prop))
        {
            return 0m;
        }

        if (prop.ValueKind == JsonValueKind.Null)
        {
            return 0m;
        }

        return prop.ValueKind switch
        {
            JsonValueKind.Number => prop.TryGetDecimal(out var d) ? d : (decimal)prop.GetDouble(),
            JsonValueKind.String => decimal.TryParse(prop.GetString(), out var d) ? d : 0m,
            _ => 0m,
        };
    }

    private static string? GetString(JsonElement obj, string propertyName)
    {
        if (!obj.TryGetProperty(propertyName, out var prop))
        {
            return null;
        }

        return prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;
    }
}
