using System.Text.Json;

namespace BodyStack.Server.Application.Suunto;

public sealed record SuuntoSleepEntry(DateTimeOffset Timestamp, SuuntoSleepEntryData EntryData);

public sealed record SuuntoSleepEntryData(
    long SleepId,
    double DurationSeconds,
    bool IsNap,
    double? DeepSleepDurationSeconds,
    double? LightSleepDurationSeconds,
    double? RemSleepDurationSeconds)
{
    public static SuuntoSleepEntryData FromJsonElement(JsonElement element)
    {
        long ReadLongOrZero(string name)
        {
            if (!element.TryGetProperty(name, out var p)) return 0;
            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt64(out var v)) return v;
            if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var d)) return (long)d;
            return 0;
        }

        double ReadDoubleOrZero(string name)
        {
            if (!element.TryGetProperty(name, out var p)) return 0;
            if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var v)) return v;
            return 0;
        }

        double? ReadDoubleNullable(string name)
        {
            if (!element.TryGetProperty(name, out var p)) return null;
            if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var v)) return v;
            return null;
        }

        bool ReadBool(string name)
        {
            if (!element.TryGetProperty(name, out var p)) return false;
            if (p.ValueKind == JsonValueKind.True) return true;
            if (p.ValueKind == JsonValueKind.False) return false;
            return false;
        }

        return new SuuntoSleepEntryData(
            SleepId: ReadLongOrZero("sleepId"),
            DurationSeconds: ReadDoubleOrZero("duration"),
            IsNap: ReadBool("isNap"),
            DeepSleepDurationSeconds: ReadDoubleNullable("deepSleepDuration"),
            LightSleepDurationSeconds: ReadDoubleNullable("lightSleepDuration"),
            RemSleepDurationSeconds: ReadDoubleNullable("remSleepDuration"));
    }
}

public sealed record SuuntoDailySleepSummary(
    string Date,
    double TotalSleepSeconds,
    double NightSleepSeconds,
    double NapSleepSeconds,
    int SleepSessionsCount,
    int NapSessionsCount);
