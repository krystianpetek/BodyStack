# Proposal: Suunto Workouts Integration with BMR and Calorie Calculations

## Overview

Extend the BodyStack Suunto integration to fetch and display workout data from the Suunto API, while fixing calorie calculation inaccuracies and adding Basal Metabolic Rate (BMR) calculations for accurate daily energy expenditure tracking.

## Business Context

### Current State
- Suunto integration fetches daily activity and sleep summaries
- Activity calorie data appears inflated (raw values from API need conversion)
- No workout-level detail view available
- No BMR calculation for complete daily energy picture

### Problem Statement
1. **Calorie Calculation Bug**: Energy values from Suunto activity API are in joules (or another unit) and need conversion to kcal. Current raw values like `410306.38` should be ~98 kcal.
2. **Missing Workout Data**: Users cannot view detailed workout information (duration, heart rate zones, calories burned per workout).
3. **Incomplete Energy Picture**: Daily calorie totals don't include BMR (basal metabolic rate), which is essential for understanding total daily energy expenditure (TDEE).

### Proposed Solution
1. Fix calorie calculation by dividing energy values by 4186 (conversion factor: 1 kcal = 4186 joules)
2. Add new API endpoint to fetch workout data from `https://api.sports-tracker.com/apiserver/v1/workouts`
3. Calculate BMR using Mifflin-St Jeor equation based on user profile data
4. Display workouts in a visually appealing UI with summary statistics
5. Show total daily calories = BMR + Activity Calories + Workout Calories

## Business Value

### User Benefits
- **Accurate calorie tracking**: Users see realistic calorie numbers instead of inflated values
- **Workout insights**: Detailed view of each workout with metrics (duration, HR zones, calories)
- **Complete energy picture**: Understanding of total daily energy expenditure including BMR
- **Better fitness decisions**: Data-driven insights for training and nutrition

### Technical Benefits
- Consistent data format across all integrations
- Reusable BMR calculation service for future features
- Enhanced user engagement with detailed workout views

## Success Criteria

1. ✅ Calorie values display correctly in kcal (e.g., 98 kcal instead of 410306)
2. ✅ Workout data fetches successfully from Suunto API
3. ✅ BMR calculates correctly using user profile (weight, height, age, gender)
4. ✅ UI displays workouts with key metrics (duration, calories, HR zones)
5. ✅ Daily summary includes: BMR + Activity + Workouts
6. ✅ All existing tests pass + new tests added

## Scope

### In Scope
- Fix existing calorie calculation in SuuntoGetDailyActivitySummaryUseCase
- Create SuuntoGetWorkoutsUseCase with API client method
- Create BMR calculation service
- Create workout API endpoint
- Create React components for workout display
- Add TypeScript types for workout data
- Unit and integration tests

### Out of Scope
- User profile management UI (assume data available)
- Historical data migration
- Workout editing or creation
- Integration with other fitness platforms

## Risks and Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| API changes | High | Log raw responses for debugging |
| User profile data missing | Medium | Use default values with warning |
| Calculation errors | High | Unit tests with known values |
| Performance with many workouts | Low | Pagination + caching |

## Timeline

**Estimated Effort**: 4-6 hours
- Backend changes: 2-3 hours
- Frontend components: 1-2 hours
- Testing: 1 hour

## Stakeholders

- End Users: Fitness enthusiasts using Suunto watches
- Development Team: AI Agent Team
- Product Owner: BodyStack Product Team

## Approval

| Role | Name | Status | Date |
|------|------|--------|------|
| Product Owner | AI Agent | Pending | - |
| Solution Architect | AI Agent | Pending | - |
| Tech Lead | AI Agent | Pending | - |
| QA Engineer | AI Agent | Pending | - |

## Next Steps

1. Review and approve proposal
2. Create technical design
3. Define test specifications
4. Implement changes
5. Verify and deploy
