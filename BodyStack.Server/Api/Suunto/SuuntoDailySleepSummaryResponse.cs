namespace BodyStack.Server.Api.Suunto;

public sealed record SuuntoDailySleepSummaryResponse(
    SuuntoDailySleepResponse[] Days,
    double TotalSleepSeconds);
