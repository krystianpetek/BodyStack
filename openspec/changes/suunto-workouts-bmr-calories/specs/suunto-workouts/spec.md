# Specifications: Suunto Workouts, BMR, and Calorie Calculations

## Feature: Suunto Workouts Integration

### Story
**As a** BodyStack user with a Suunto watch  
**I want to** view my workout history with detailed metrics  
**So that** I can track my fitness progress and understand my energy expenditure

### Acceptance Criteria

#### Scenario 1: View Workout List
**Given** I am authenticated with Suunto  
**When** I navigate to the Suunto dashboard  
**Then** I see a list of my recent workouts with basic information

#### Scenario 2: View Workout Details
**Given** I am viewing the workout list  
**When** I click on a specific workout  
**Then** I see detailed metrics including:
- Workout duration
- Total distance
- Calories burned
- Heart rate zones
- Elevation gain/loss

#### Scenario 3: Filter Workouts by Date
**Given** I am viewing the workout list  
**When** I select a date range  
**Then** only workouts within that range are displayed

## Feature: Calorie Calculation Fix

### Story
**As a** BodyStack user  
**I want to** see accurate calorie values in kcal  
**So that** I can properly track my energy expenditure

### Acceptance Criteria

#### Scenario 1: Correct Calorie Display
**Given** I am viewing my daily activity summary  
**When** the data includes energy values  
**Then** calories are displayed in kcal (divided by 4186 from raw joules)

**Examples:**
| Raw Value | Displayed Value |
|-----------|-----------------|
| 410306.38 | 98 kcal |
| 376812.0 | 90 kcal |
| 184219.25 | 44 kcal |

#### Scenario 2: Existing Data Compatibility
**Given** previously cached data exists  
**When** I view the activity summary  
**Then** the corrected calculation is applied retroactively

## Feature: BMR Calculation

### Story
**As a** fitness-conscious user  
**I want to** see my Basal Metabolic Rate  
**So that** I understand my total daily energy expenditure

### Acceptance Criteria

#### Scenario 1: Calculate BMR
**Given** I have provided my profile data (weight, height, age, gender)  
**When** I view my daily summary  
**Then** my BMR is calculated using Mifflin-St Jeor formula

**Examples:**
| Weight | Height | Age | Gender | BMR |
|--------|--------|-----|--------|-----|
| 70 kg | 175 cm | 30 | Male | ~1641 kcal |
| 60 kg | 165 cm | 25 | Female | ~1380 kcal |

#### Scenario 2: Display Daily Energy Summary
**Given** I am viewing my daily summary  
**When** all data is loaded  
**Then** I see:
- BMR calories
- Activity calories
- Workout calories
- Total calories (sum of all)

## API Specifications

### GET /api/suunto/workouts

#### Request
```
GET /api/suunto/workouts?from=2026-02-01&to=2026-02-15&ttlMinutes=15
Headers:
  sttauthorization: <token>
```

#### Response (200 OK)
```json
{
  "workouts": [
    {
      "activityId": 23,
      "startTime": "2026-02-15T08:00:00.000+01:00",
      "totalTime": 3600,
      "totalDistance": 10000,
      "totalAscent": 150,
      "totalDescent": 140,
      "calories": 450,
      "avgHeartRate": 145,
      "maxHeartRate": 175,
      "workoutType": "Running",
      "heartRateZones": [
        {"zone": 1, "time": 600, "percent": 16.7},
        {"zone": 2, "time": 1800, "percent": 50.0},
        {"zone": 3, "time": 1200, "percent": 33.3}
      ]
    }
  ],
  "totalCount": 15,
  "totalCalories": 6750
}
```

#### Error Responses
- **401 Unauthorized**: Invalid or missing authentication
- **400 Bad Request**: Invalid date format or range
- **502 Bad Gateway**: Suunto API error
- **500 Internal Error**: Server error

### GET /api/suunto/daily-summary

#### Request
```
GET /api/suunto/daily-summary?date=2026-02-15
Headers:
  sttauthorization: <token>
```

#### Response (200 OK)
```json
{
  "date": "2026-02-15",
  "bmrCalories": 1641,
  "activityCalories": 450,
  "workoutCalories": 680,
  "totalCalories": 2771
}
```

## Technical Specifications

### Data Validation

#### Date Parameters
- `from` and `to` must be valid dates (yyyy-MM-dd)
- Date range cannot exceed 90 days
- `to` must be equal or later than `from`

#### Authentication
- `sttauthorization` header required
- Must be non-empty string

### Performance Requirements

#### Response Times
- Workouts API: < 2 seconds (with cache)
- Daily Summary API: < 1 second

#### Cache Requirements
- Memory cache TTL: 15 minutes (configurable)
- File cache TTL: Configurable via ttlMinutes parameter

### Security Requirements

#### Authentication
- Verify STTAuthorization header present
- Return 401 if missing or invalid

#### Data Privacy
- Don't log authentication tokens
- Don't expose user IDs in error messages

## UI Specifications

### Workout Card Component

**Layout:**
```
┌─────────────────────────────────────┐
│ 🏃 Running                    08:00 │
│ Feb 15, 2026                        │
├─────────────────────────────────────┤
│ ⏱️ 1:00:00    📏 10.0 km   🔥 450  │
│ ❤️ Avg: 145   ❤️ Max: 175          │
├─────────────────────────────────────┤
│ HR Zones: ▓▓▓▓░░░░▓▓▓▓▓▓▓▓░░░░░░  │
└─────────────────────────────────────┘
```

**Fields:**
- Workout type icon
- Start time and date
- Duration (formatted as HH:MM:SS)
- Distance (km with 1 decimal)
- Calories (whole number)
- Avg/Max HR (if available)
- Heart rate zone bar chart

### Daily Energy Summary Component

**Layout:**
```
Daily Energy Summary - Feb 15, 2026
┌─────────────────────────────────────┐
│ 🛌 BMR:           1,641 kcal       │
│ 🚶 Activity:        450 kcal       │
│ 🏃 Workouts:        680 kcal       │
├─────────────────────────────────────┤
│ 🔥 Total:         2,771 kcal       │
└─────────────────────────────────────┘
```

**Behavior:**
- Numbers animate when data loads
- Each row has distinct color/icon
- Total is bold and prominent

## Test Scenarios

### Backend Tests

#### Unit Test: Calorie Conversion
```gherkin
Given raw energy value is 410306.38 joules
When converted to kcal
Then result should be approximately 98 kcal
```

#### Unit Test: BMR Calculation
```gherkin
Given user is male, 70kg, 175cm, 30 years old
When BMR is calculated
Then result should be approximately 1641 kcal
```

#### Integration Test: Workouts API
```gherkin
Given user is authenticated
And date range is valid
When GET /api/suunto/workouts is called
Then response contains workouts list
And status code is 200
```

### Frontend Tests

#### Component Test: WorkoutCard
```gherkin
Given workout data with all fields
When WorkoutCard is rendered
Then all metrics are displayed correctly
And HR zones bar is visible
```

#### Component Test: DailySummary
```gherkin
Given BMR, activity, and workout calories
When DailySummary is rendered
Then total is sum of all three
And values are formatted with commas
```

## Definition of Done

- [ ] All acceptance criteria met
- [ ] API endpoints return correct data
- [ ] Calorie calculations accurate (±1 kcal tolerance)
- [ ] BMR calculation accurate (±5 kcal tolerance)
- [ ] UI components render correctly
- [ ] Unit tests pass (>80% coverage)
- [ ] Integration tests pass
- [ ] No console errors
- [ ] Responsive design works on mobile
- [ ] Documentation updated

## Open Questions

1. **Q**: Should we store user profile (weight, height, etc.) in database or calculate on-the-fly?
   **A**: Calculate on-the-fly from query parameters or headers for now

2. **Q**: How to handle missing HR data in older workouts?
   **A**: Display "--" or hide HR section

3. **Q**: Should workouts be cached separately from activity data?
   **A**: Yes, separate cache keys for flexibility
