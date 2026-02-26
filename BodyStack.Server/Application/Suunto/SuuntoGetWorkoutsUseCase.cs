using System.Globalization;
using System.IO.Compression;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BodyStack.Server.Application.Suunto.Models;
using BodyStack.Server.Integrations.Suunto;
using Microsoft.Extensions.Caching.Memory;

namespace BodyStack.Server.Application.Suunto;

public sealed class SuuntoGetWorkoutsUseCase
{
    private readonly ISuuntoWorkoutClient _client;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SuuntoGetWorkoutsUseCase> _logger;

    public SuuntoGetWorkoutsUseCase(
        ISuuntoWorkoutClient client,
        IMemoryCache cache,
        ILogger<SuuntoGetWorkoutsUseCase> logger)
    {
        _client = client;
        _cache = cache;
        _logger = logger;
    }

    public async Task<SuuntoWorkoutsResponse> ExecuteAsync(
        string sttAuthorization,
        TimeSpan ttl,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(sttAuthorization);
        var cacheKey = $"suunto.workouts.{tokenHash}.{from?.ToString("yyyy-MM-dd") ?? "_"}.{to?.ToString("yyyy-MM-dd") ?? "_"}";

        if (_cache.TryGetValue(cacheKey, out SuuntoWorkoutsResponse? cached) && cached is not null)
        {
            _logger.LogDebug("Returning cached workouts for {TokenHash}", tokenHash);
            return cached;
        }

        var json = await _client.GetWorkoutsAsync(sttAuthorization, cancellationToken);

        var workouts = ParseWorkouts(json, from, to);

        var response = new SuuntoWorkoutsResponse(
            workouts,
            workouts.Count,
            workouts.Sum(w => w.Calories ?? 0));

        _cache.Set(cacheKey, response, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        });

        return response;
    }

    private IReadOnlyList<SuuntoWorkout> ParseWorkouts(JsonDocument json, DateOnly? from, DateOnly? to)
    {
        var workouts = new List<SuuntoWorkout>();

        if (!json.RootElement.TryGetProperty("payload", out var payload))
        {
            _logger.LogWarning("No payload in workouts response");
            return workouts;
        }

        foreach (var element in payload.EnumerateArray())
        {
            try
            {
                var workout = ParseWorkout(element);

                // Filter by date if specified
                var workoutDate = DateOnly.FromDateTime(workout.StartTime.LocalDateTime.Date);
                if (from.HasValue && workoutDate < from.Value) continue;
                if (to.HasValue && workoutDate > to.Value) continue;

                workouts.Add(workout);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse workout entry");
            }
        }

        return workouts.OrderByDescending(w => w.StartTime).ToArray();
    }

    private SuuntoWorkout ParseWorkout(JsonElement element)
    {
        var activityId = element.GetProperty("activityId").GetInt64();
        var startTime = DateTimeOffset.FromUnixTimeMilliseconds(element.GetProperty("startTime").GetInt64());
        var totalTime = element.GetProperty("totalTime").GetDouble();
        var totalDistance = element.GetProperty("totalDistance").GetDouble();
        var totalAscent = element.GetProperty("totalAscent").GetDouble();
        var totalDescent = element.GetProperty("totalDescent").GetDouble();
        var energyConsumption = element.GetProperty("energyConsumption").GetDouble();

        // Parse extensions
        var extensions = ParseExtensions(element);

        // Parse heart rate zones
        var hrZones = ParseHeartRateZones(element);

        return new SuuntoWorkout(
            ActivityId: activityId,
            StartTime: startTime,
            TotalTimeSeconds: totalTime,
            TotalDistance: totalDistance,
            TotalAscent: totalAscent,
            TotalDescent: totalDescent,
            Calories: energyConsumption, //extensions?.PeakEpoc != null ? CalculateCaloriesFromEpoc(extensions.PeakEpoc.Value) : null,
            AvgHeartRate: null, // Will be calculated from zones if needed
            MaxHeartRate: extensions?.MaxHeartRate,
            WorkoutType: null,
            StepCount: null,
            HeartRateZones: hrZones,
            Extensions: extensions);
    }

    private SuuntoWorkoutExtensions? ParseExtensions(JsonElement element)
    {
        if (!element.TryGetProperty("extensions", out var extensions))
            return null;

        double? maxHr = null;
        double? vo2Max = null;
        double? peakEpoc = null;
        double? recoveryTime = null;
        double? minTemp = null;
        double? avgTemp = null;
        double? maxTemp = null;
        string? feeling = null;

        foreach (var ext in extensions.EnumerateArray())
        {
            var type = ext.GetProperty("type").GetString();

            switch (type)
            {
                case "FitnessExtension":
                    if (ext.TryGetProperty("maxHeartRate", out var maxHrEl))
                        maxHr = maxHrEl.GetDouble();
                    if (ext.TryGetProperty("vo2Max", out var vo2El) && vo2El.ValueKind != JsonValueKind.Null)
                        vo2Max = vo2El.GetDouble();
                    break;

                case "IntensityExtension":
                    // HR zones handled separately
                    break;

                case "SummaryExtension":
                    if (ext.TryGetProperty("peakEpoc", out var epocEl) && epocEl.ValueKind != JsonValueKind.Null)
                        peakEpoc = epocEl.GetDouble();
                    if (ext.TryGetProperty("recoveryTime", out var recEl) && recEl.ValueKind != JsonValueKind.Null)
                        recoveryTime = recEl.GetDouble();
                    if (ext.TryGetProperty("minTemperature", out var minTempEl))
                        minTemp = minTempEl.GetDouble();
                    if (ext.TryGetProperty("avgTemperature", out var avgTempEl))
                        avgTemp = avgTempEl.GetDouble();
                    if (ext.TryGetProperty("maxTemperature", out var maxTempEl))
                        maxTemp = maxTempEl.GetDouble();
                    if (ext.TryGetProperty("feeling", out var feelEl) && feelEl.ValueKind != JsonValueKind.Null)
                        feeling = feelEl.GetString();
                    break;
            }
        }

        return new SuuntoWorkoutExtensions(
            maxHr, vo2Max, peakEpoc, recoveryTime, minTemp, avgTemp, maxTemp, feeling);
    }

    private IReadOnlyList<SuuntoHeartRateZone> ParseHeartRateZones(JsonElement element)
    {
        var zones = new List<SuuntoHeartRateZone>();

        if (!element.TryGetProperty("extensions", out var extensions))
            return zones;

        foreach (var ext in extensions.EnumerateArray())
        {
            if (ext.GetProperty("type").GetString() != "IntensityExtension")
                continue;

            if (!ext.TryGetProperty("zones", out var zonesEl) ||
                !zonesEl.TryGetProperty("heartRate", out var hrZones))
                continue;

            var totalTime = 0.0;
            var zoneTimes = new Dictionary<int, double>();

            for (int i = 1; i <= 5; i++)
            {
                var zoneKey = $"zone{i}";
                if (!hrZones.TryGetProperty(zoneKey, out var zoneEl))
                    continue;

                var time = zoneEl.GetProperty("totalTime").GetDouble();
                var lower = zoneEl.GetProperty("lowerLimit").GetDouble();

                zoneTimes[i] = time;
                totalTime += time;
            }

            foreach (var kvp in zoneTimes)
            {
                var percent = totalTime > 0 ? (kvp.Value / totalTime) * 100 : 0;
                zones.Add(new SuuntoHeartRateZone(
                    kvp.Key,
                    hrZones.GetProperty($"zone{kvp.Key}").GetProperty("lowerLimit").GetDouble(),
                    kvp.Value,
                    percent));
            }
        }

        return zones;
    }

    private double CalculateCaloriesFromEpoc(double epoc)
    {
        // EPOC (Excess Post-exercise Oxygen Consumption) can be used to estimate calories
        // This is a simplified conversion - in reality this would need more physiological data
        // For now, return a placeholder that can be refined later
        return epoc * 0.5; // Rough estimate: 0.5 kcal per EPOC unit
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
