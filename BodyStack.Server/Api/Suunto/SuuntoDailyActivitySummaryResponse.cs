namespace BodyStack.Server.Api.Suunto;

public sealed record SuuntoDailyActivitySummaryResponse(
    SuuntoDailyActivityResponse[] Days,
    int TotalSteps,
    double TotalEnergyConsumption);
