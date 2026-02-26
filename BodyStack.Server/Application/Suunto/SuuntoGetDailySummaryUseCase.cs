using System.Globalization;
using BodyStack.Server.Application.Suunto.Models;
using BodyStack.Server.Domain.Services;
using Microsoft.Extensions.Caching.Memory;

namespace BodyStack.Server.Application.Suunto;

/// <summary>
/// Use case for calculating daily energy summary including BMR, activity, and workouts
/// </summary>
public sealed class SuuntoGetDailySummaryUseCase
{
    private readonly SuuntoGetDailyActivitySummaryUseCase _activityUseCase;
    private readonly SuuntoGetWorkoutsUseCase _workoutsUseCase;
    private readonly BmrCalculator _bmrCalculator;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SuuntoGetDailySummaryUseCase> _logger;

    public SuuntoGetDailySummaryUseCase(
        SuuntoGetDailyActivitySummaryUseCase activityUseCase,
        SuuntoGetWorkoutsUseCase workoutsUseCase,
        BmrCalculator bmrCalculator,
        IMemoryCache cache,
        ILogger<SuuntoGetDailySummaryUseCase> logger)
    {
        _activityUseCase = activityUseCase;
        _workoutsUseCase = workoutsUseCase;
        _bmrCalculator = bmrCalculator;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Calculates daily energy summary for a specific date
    /// </summary>
    public async Task<SuuntoDailyEnergySummary> ExecuteAsync(
        string sttAuthorization,
        DateOnly date,
        double weightKg,
        double heightCm,
        int age,
        string gender,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"suunto.dailysummary.{date:yyyy-MM-dd}.{weightKg}.{heightCm}.{age}.{gender}";
        
        if (_cache.TryGetValue(cacheKey, out SuuntoDailyEnergySummary? cached) && cached is not null)
        {
            _logger.LogDebug("Returning cached daily summary for {Date}", date);
            return cached;
        }

        // Calculate BMR
        var bmr = _bmrCalculator.CalculateBmr(weightKg, heightCm, age, gender);
        var bmrForDay = bmr; // Full day BMR

        // Get activity calories for the day
        var activityData = await _activityUseCase.ExecuteAsync(
            sttAuthorization, 
            ttl, 
            date, 
            date, 
            cancellationToken);
        
        var activityCalories = activityData.FirstOrDefault()?.EnergyConsumption ?? 0;

        // Get workout calories for the day
        var workoutsData = await _workoutsUseCase.ExecuteAsync(
            sttAuthorization,
            ttl,
            date,
            date,
            cancellationToken);
        
        var workoutCalories = workoutsData.TotalCalories;

        // Calculate total
        var total = bmrForDay + activityCalories + workoutCalories;

        var summary = new SuuntoDailyEnergySummary(
            Date: date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            BmrCalories: bmrForDay,
            ActivityCalories: activityCalories,
            WorkoutCalories: workoutCalories,
            TotalCalories: total);

        _cache.Set(cacheKey, summary, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        });

        return summary;
    }
}
