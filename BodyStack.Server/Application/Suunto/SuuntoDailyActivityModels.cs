using System.Text.Json;

namespace BodyStack.Server.Application.Suunto;

public sealed record SuuntoActivityEntry(DateTimeOffset Timestamp, SuuntoActivityEntryData EntryData);

public sealed record SuuntoActivityEntryData(
    double? Hr,
    int? StepCount,
    double? EnergyConsumption,
    double? Hrv)
{
    public static SuuntoActivityEntryData FromJsonElement(JsonElement element)
    {
        double? ReadDouble(string name)
        {
            if (!element.TryGetProperty(name, out var p)) return null;
            if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var v)) return v;
            return null;
        }

        int? ReadInt(string name)
        {
            if (!element.TryGetProperty(name, out var p)) return null;
            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var v)) return v;
            if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var d)) return (int)d;
            return null;
        }

        return new SuuntoActivityEntryData(
            Hr: ReadDouble("hr"),
            StepCount: ReadInt("stepCount"),
            EnergyConsumption: ReadDouble("energyConsumption"),
            Hrv: ReadDouble("hrv"));
    }
}

public sealed record SuuntoDailyActivitySummary(
    string Date,
    int Steps,
    double EnergyConsumption,
    double? AvgHr,
    double? AvgHrv,
    int Samples);
