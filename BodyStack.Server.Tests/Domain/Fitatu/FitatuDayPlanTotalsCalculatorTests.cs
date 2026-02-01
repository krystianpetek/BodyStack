using System.Text.Json;
using BodyStack.Server.Domain.Fitatu;
using Xunit;

namespace BodyStack.Server.Tests.Domain.Fitatu;

public sealed class FitatuDayPlanTotalsCalculatorTests
{
    [Fact]
    public void Compute_WhenFiberSugarsSaltAreMissing_TreatsThemAsZero()
    {
        const string json = """
        {
          "dietPlan": {
            "breakfast": {
              "mealName": "Breakfast",
              "mealTime": "08:00",
              "items": [
                { "energy": 100, "protein": 10, "fat": 1, "carbohydrate": 20 },
                { "energy": 50, "protein": 5, "fat": 2, "carbohydrate": 3, "fiber": null, "sugars": null, "salt": null }
              ]
            },
            "lunch": {
              "mealName": "Lunch",
              "items": [
                { "energy": 200, "protein": 0, "fat": 10, "carbohydrate": 0, "fiber": 7, "sugars": 1.5, "salt": 0.1 }
              ]
            }
          }
        }
        """;

        using var doc = JsonDocument.Parse(json);
        var sut = new FitatuDayPlanTotalsCalculator();

        var result = sut.Compute(doc);

        Assert.Equal(350m, result.Totals.Energy);
        Assert.Equal(15m, result.Totals.Protein);
        Assert.Equal(13m, result.Totals.Fat);
        Assert.Equal(23m, result.Totals.Carbohydrate);
        Assert.Equal(7m, result.Totals.Fiber);
        Assert.Equal(1.5m, result.Totals.Sugars);
        Assert.Equal(0.1m, result.Totals.Salt);

        var breakfast = Assert.Single(result.Meals, m => m.MealKey == "breakfast");
        Assert.Equal(150m, breakfast.Totals.Energy);
        Assert.Equal(0m, breakfast.Totals.Fiber);
    }
}
