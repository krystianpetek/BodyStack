namespace BodyStack.Server.Api.Suunto;

public sealed record SuuntoDailySleepResponse(
    string Date,
    double TotalSleepSeconds,
    double NightSleepSeconds,
    double NapSleepSeconds,
    int SleepSessionsCount,
    int NapSessionsCount);
