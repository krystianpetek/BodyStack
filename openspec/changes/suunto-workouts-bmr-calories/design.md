# Design: Suunto Workouts, BMR Calculation, and Calorie Fix

## Architecture Overview

This change extends the existing Suunto integration with three main components:
1. **Calorie Fix**: Convert joules to kcal in activity data
2. **Workouts API**: Fetch and aggregate workout data
3. **BMR Service**: Calculate basal metabolic rate for users

## Technical Decisions

### 1. Calorie Conversion

**Problem**: Suunto API returns energy in joules (1 kcal = 4186 joules)

**Solution**: Divide energy values by 4186 in the aggregation logic

**Location**: `SuuntoGetDailyActivitySummaryUseCase.cs` line 162

```csharp
// Current (incorrect)
acc.Energy += energy;

// Fixed (correct)
acc.Energy += energy / 4186.0;
```

**Rationale**: 
- Minimal change to existing code
- Conversion happens at data ingestion point
- Consistent with physics (1 kcal = 4186 joules)

### 2. Workout Data Fetching

**API Endpoint**: `GET https://api.sports-tracker.com/apiserver/v1/workouts?limited=true&limit=1000000`

**Authentication**: Same as existing Suunto endpoints (`sttauthorization` header)

**Architecture Pattern**:
```
Client Request
    │
    ▼
SuuntoGetWorkoutsUseCase
    │
    ├─► Validate input (date range)
    ├─► Check cache (MemoryCache with TTL)
    │
    ▼
SuuntoWorkoutClient
    │
    ├─► HTTP GET with auth header
    ├─► Handle gzip compression
    ├─► Parse JSON response
    │
    ▼
Response DTO
    │
    ├─► Map to domain model
    ├─► Calculate derived metrics
    │
    ▼
API Response
```

**New Components**:
- `SuuntoWorkoutClient` - HTTP client for workout API
- `SuuntoGetWorkoutsUseCase` - Business logic for fetching workouts
- `WorkoutModels` - DTOs for workout data

### 3. BMR Calculation

**Formula**: Mifflin-St Jeor Equation

```
Men: BMR = (10 × weight in kg) + (6.25 × height in cm) - (5 × age in years) + 5
Women: BMR = (10 × weight in kg) + (6.25 × height in cm) - (5 × age in years) - 161
```

**Implementation**:
```csharp
public class BmrCalculator
{
    public double CalculateBmr(double weightKg, double heightCm, int age, string gender)
    {
        var bmr = (10 * weightKg) + (6.25 * heightCm) - (5 * age);
        return gender.ToLower() switch
        {
            "male" => bmr + 5,
            "female" => bmr - 161,
            _ => bmr + 5 // Default to male formula
        };
    }
    
    public double CalculateDailyBmr(double hourlyBmr) => hourlyBmr * 24;
}
```

**Integration Point**:
- New endpoint: `GET /api/suunto/daily-summary?date=YYYY-MM-DD`
- Returns: `{ bmr, activityCalories, workoutCalories, totalCalories }`

### 4. Data Models

**Workout Model**:
```csharp
public record SuuntoWorkout(
    long ActivityId,
    DateTimeOffset StartTime,
    double TotalTimeSeconds,
    double TotalDistance,
    double TotalAscent,
    double TotalDescent,
    double? MaxHeartRate,
    double? AvgHeartRate,
    double? Calories,
    string? WorkoutType,
    int? StepCount,
    List<SuuntoHeartRateZone> HeartRateZones,
    SuuntoWorkoutExtensions Extensions);

public record SuuntoHeartRateZone(
    int Zone,
    double LowerLimit,
    double TotalTimeSeconds);

public record SuuntoWorkoutExtensions(
    double? MaxHeartRate,
    double? Vo2Max,
    double? PeakEpoc,
    double? RecoveryTime,
    string? Feeling);
```

**Daily Summary Model**:
```csharp
public record SuuntoDailyEnergySummary(
    string Date,
    double BmrCalories,
    double ActivityCalories,
    double WorkoutCalories,
    double TotalCalories);
```

### 5. Frontend Architecture

**New Components**:
1. `SuuntoWorkoutsList` - Display list of workouts
2. `SuuntoWorkoutCard` - Individual workout card with metrics
3. `SuuntoDailyEnergySummary` - Daily calorie breakdown
4. `HeartRateZonesChart` - Visual representation of HR zones

**Component Hierarchy**:
```
SuuntoPage
    ├── SuuntoAuthStatus
    ├── SuuntoDailyEnergySummary
    │   ├── BmrIndicator
    │   ├── ActivityCaloriesIndicator
    │   ├── WorkoutCaloriesIndicator
    │   └── TotalCaloriesIndicator
    ├── SuuntoWorkoutsList
    │   └── SuuntoWorkoutCard[]
    │       ├── WorkoutHeader (date, type, duration)
    │       ├── WorkoutMetrics (distance, calories, HR)
    │       └── HeartRateZonesBar
    └── SuuntoActivityChart
```

**Data Flow**:
```
SuuntoPage mounts
    │
    ├─► Fetch daily summary (BMR + activity + workouts)
    │   API: GET /api/suunto/daily-summary?date=today
    │
    ├─► Fetch workouts list
    │   API: GET /api/suunto/workouts?from=date&to=date
    │
    ▼
Update state
    │
    ▼
Render components
```

## API Endpoints

### 1. Get Workouts
```
GET /api/suunto/workouts
Query Parameters:
  - from: Date (optional, default: 7 days ago)
  - to: Date (optional, default: today)
  - ttlMinutes: int (optional, default: 15)

Headers:
  - sttauthorization: string (required)

Response:
{
  "workouts": [
    {
      "activityId": 12345,
      "startTime": "2026-02-15T08:00:00.000+01:00",
      "totalTime": 3600,
      "totalDistance": 10000,
      "calories": 450,
      "avgHeartRate": 145,
      "maxHeartRate": 175,
      "heartRateZones": [
        {"zone": 1, "time": 600, "percent": 17},
        {"zone": 2, "time": 1800, "percent": 50},
        {"zone": 3, "time": 1200, "percent": 33}
      ]
    }
  ],
  "totalCount": 15,
  "totalCalories": 6750
}
```

### 2. Get Daily Energy Summary
```
GET /api/suunto/daily-summary
Query Parameters:
  - date: Date (required, format: yyyy-MM-dd)

Headers:
  - sttauthorization: string (required)

Response:
{
  "date": "2026-02-15",
  "bmrCalories": 1641,
  "activityCalories": 450,
  "workoutCalories": 680,
  "totalCalories": 2771
}
```

## Security Considerations

1. **Authentication**: Use existing STTAuthorization header pattern
2. **Data Validation**: Validate date ranges (max 90 days)
3. **Rate Limiting**: Cache responses to minimize API calls
4. **Error Handling**: Don't expose internal error details to client

## Performance Considerations

1. **Caching Strategy**:
   - File cache for raw API responses (TTL: configurable)
   - Memory cache for aggregated data (TTL: 15 minutes)
   
2. **Pagination**:
   - API returns up to 1M workouts (sufficient for most users)
   - Frontend implements virtual scrolling for large lists

3. **Lazy Loading**:
   - Workout details loaded on demand
   - HR zone data computed client-side from raw data

## Error Handling

### Backend Errors
```csharp
try
{
    var workouts = await useCase.ExecuteAsync(auth, from, to, ct);
    return Results.Ok(workouts);
}
catch (SuuntoUnauthorizedException)
{
    return Results.Unauthorized();
}
catch (SuuntoApiException ex)
{
    return Results.StatusCode(502); // Bad Gateway
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to fetch workouts");
    return Results.Problem("Failed to fetch workout data");
}
```

### Frontend Errors
- Show error toast for API failures
- Display cached data with "last updated" timestamp
- Retry button for transient errors

## Testing Strategy

### Unit Tests
1. BMR calculation with known values
2. Calorie conversion (joules to kcal)
3. Workout data parsing from JSON
4. Date range filtering

### Integration Tests
1. Full API endpoint test with mocked Suunto client
2. Cache behavior verification
3. Error scenario handling

### Frontend Tests
1. Component rendering with mock data
2. API integration tests
3. User interaction flows

## Dependencies

### Backend
- Existing: Suunto client infrastructure
- New: None (uses existing HTTP client pattern)

### Frontend
- Existing: React, TypeScript, Tailwind CSS
- New: Recharts (for HR zone visualization) - optional

## Rollback Plan

1. Feature flags for new endpoints
2. Database: No migrations (read-only data)
3. Revert: Remove endpoints and components

## Deployment Checklist

- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] Frontend build succeeds
- [ ] Manual testing completed
- [ ] Documentation updated
- [ ] Feature flags configured (if needed)

## Future Enhancements

1. Workout comparison over time
2. Training load calculations
3. Integration with training plans
4. Export workout data to GPX/TCX

## Decisions Log

| Decision | Rationale | Date |
|----------|-----------|------|
| Divide by 4186 | 1 kcal = 4186 joules | 2026-02-15 |
| Mifflin-St Jeor | Industry standard BMR formula | 2026-02-15 |
| Separate workout endpoint | Clean API design, caching benefits | 2026-02-15 |
| Memory cache for BMR | User profile rarely changes | 2026-02-15 |

## Approval

| Role | Decision | Notes |
|------|----------|-------|
| Solution Architect | Approved | Clean extension of existing architecture |
| Tech Lead | Approved | Implement in existing pattern |
| Security Engineer | Approved | No new security concerns |
| DevOps Engineer | Approved | No infrastructure changes |
