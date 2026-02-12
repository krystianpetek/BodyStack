## Why

BodyStack codebase contains critical code quality issues that threaten stability, security, and maintainability. Key problems include: fragile exception handling using string comparison, SignalR broadcasting to all users (privacy leak), inefficient database queries loading entire tables into memory, and minimal test coverage (only 3 tests). These issues must be addressed before adding new features to prevent technical debt accumulation and production incidents.

## What Changes

### Backend Critical Fixes
- **Create custom exception hierarchy** in Domain layer (FitatuSessionNotFoundException, MonthExportIncompleteException, etc.)
- **Replace exception message comparisons** in Program.cs with proper exception types
- **Fix FitatuSessionRepository.GetLatestAsync()** - optimize query to avoid loading all records
- **Isolate SignalR by user** - use Groups instead of broadcasting to all clients
- **Standardize error responses** - consistent format with error codes, no stack traces in production

### Frontend Refactoring
- **Create date utility module** - consolidate duplicated date formatting logic
- **Split DashboardShell component** (224 lines) into smaller, focused components
- **Consolidate API error handling** - uniform error handling across all API calls
- **Extract shared UI constants** - reusable Tailwind class combinations

### Testing Improvements
- **Add repository tests** using in-memory EF Core
- **Add integration tests** for critical API endpoints
- **Setup Vitest** for frontend testing
- **Add component tests** for critical UI components

### Code Quality
- **Remove unused code** - WeatherForecast template endpoint
- **Add documentation** - exception hierarchy and patterns
- **Performance monitoring** - query execution benchmarks

## Capabilities

### New Capabilities
- `exception-handling`: Custom exception types and standardized error responses
- `signalr-user-isolation`: SignalR groups per user for privacy and scalability
- `repository-optimization`: Efficient database queries with proper indexing
- `frontend-component-architecture`: Modular, testable React components
- `test-coverage`: Unit and integration tests for critical paths

### Modified Capabilities
- None - this is a pure refactoring with no behavioral changes to existing features

## Impact

### Backend Files
- `BodyStack.Server/Domain/Exceptions/` (new directory)
- `BodyStack.Server/Program.cs` (exception handling updates)
- `BodyStack.Server/Infrastructure/Persistence/FitatuSessionRepository.cs` (query optimization)
- `BodyStack.Server/Realtime/FitatuMonthRecalculationWorker.cs` (SignalR isolation)
- `BodyStack.Server/Application/` (catch blocks updated)

### Frontend Files
- `bodystack.client/src/utils/date.ts` (new)
- `bodystack.client/src/api/errorHandling.ts` (new)
- `bodystack.client/src/styles/constants.ts` (new)
- `bodystack.client/src/pages/DashboardShell.tsx` (split into components)
- `bodystack.client/src/pages/FitatuPage.tsx` (use new utilities)

### Test Files
- `BodyStack.Server.Tests/Domain/Exceptions/` (new)
- `BodyStack.Server.Tests/Infrastructure/Persistence/` (new)
- `BodyStack.Server.Tests/Integration/` (new)
- `bodystack.client/src/**/*.test.ts` (new test files)

### Dependencies
- No new external dependencies
- Existing: .NET 10, EF Core 10, React 19, SignalR

### Breaking Changes
- None - all changes are internal refactoring with preserved API contracts
- SignalR message format remains the same (only routing changes)
- Database schema unchanged
- API response formats enhanced but backward compatible
