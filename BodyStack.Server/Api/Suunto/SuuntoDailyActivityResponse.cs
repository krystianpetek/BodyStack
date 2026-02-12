namespace BodyStack.Server.Api.Suunto;

public sealed record SuuntoDailyActivityResponse(
    string Date,
    int Steps,
    double EnergyConsumption,
    double? AvgHr,
    double? AvgHrv,
    int Samples);
