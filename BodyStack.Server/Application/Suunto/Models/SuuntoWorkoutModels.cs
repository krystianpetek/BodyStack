namespace BodyStack.Server.Application.Suunto.Models;

/// <summary>
/// Represents a workout from Suunto API
/// </summary>
public sealed record SuuntoWorkout(
    long ActivityId,
    DateTimeOffset StartTime,
    double TotalTimeSeconds,
    double TotalDistance,
    double TotalAscent,
    double TotalDescent,
    double? Calories,
    double? AvgHeartRate,
    double? MaxHeartRate,
    string? WorkoutType,
    int? StepCount,
    IReadOnlyList<SuuntoHeartRateZone> HeartRateZones,
    SuuntoWorkoutExtensions? Extensions);

/// <summary>
/// Heart rate zone data for a workout
/// </summary>
public sealed record SuuntoHeartRateZone(
    int Zone,
    double LowerLimit,
    double TotalTimeSeconds,
    double Percentage);

/// <summary>
/// Extended workout metrics
/// </summary>
public sealed record SuuntoWorkoutExtensions(
    double? MaxHeartRate,
    double? Vo2Max,
    double? PeakEpoc,
    double? RecoveryTime,
    double? MinTemperature,
    double? AvgTemperature,
    double? MaxTemperature,
    string? Feeling);

/// <summary>
/// API response for workouts endpoint
/// </summary>
public sealed record SuuntoWorkoutsResponse(
    IReadOnlyList<SuuntoWorkout> Workouts,
    int TotalCount,
    double TotalCalories);

/// <summary>
/// Daily energy summary combining BMR, activity, and workouts
/// </summary>
public sealed record SuuntoDailyEnergySummary(
    string Date,
    double BmrCalories,
    double ActivityCalories,
    double WorkoutCalories,
    double TotalCalories);
