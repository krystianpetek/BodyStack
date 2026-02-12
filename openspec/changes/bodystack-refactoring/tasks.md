## 1. Backend Critical Fixes - Phase 1

### 1.1 Custom Exception Types
- [x] 1.1.1 Create DomainException base class in BodyStack.Server/Domain/Exceptions/
- [x] 1.1.2 Create FitatuSessionNotFoundException with ErrorCode property
- [x] 1.1.3 Create MonthExportIncompleteException with missing days list
- [x] 1.1.4 Create UnauthorizedIntegrationException for 401/403 responses
- [x] 1.1.5 Create IntegrationApiException for general API errors

### 1.2 Exception Handling Updates
- [x] 1.2.1 Update Program.cs exception handlers to catch custom types instead of string comparison
- [x] 1.2.2 Replace `ex.Message.StartsWith("Fitatu session not found")` with FitatuSessionNotFoundException filter
- [x] 1.2.3 Replace `ex.Message.StartsWith("Month export incomplete")` with MonthExportIncompleteException filter
- [x] 1.2.4 Update all catch blocks in Application layer to throw custom exceptions
- [x] 1.2.5 Implement standardized error response middleware/format

### 1.3 Repository Query Optimization
- [x] 1.3.1 Fix FitatuSessionRepository.GetLatestAsync() - remove ToListAsync()
- [x] 1.3.2 Add OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync() pattern
- [x] 1.3.3 Review other repositories for similar anti-patterns
- [x] 1.3.4 Add query performance logging (>100ms warnings)
- [x] 1.3.5 Consider adding index on FitatuSessions.UpdatedAt column

### 1.4 SignalR User Isolation
- [x] 1.4.1 Update FitatuMonthRecalculationWorker to use hub.Clients.Group($"user-{userId}")
- [x] 1.4.2 Add JoinUserGroup method to FitatuMonthHub
- [x] 1.4.3 Modify frontend to call JoinUserGroup on connection with userId from JWT
- [ ] 1.4.4 Test with multiple concurrent users to verify isolation
- [x] 1.4.5 Handle reconnection - auto-rejoin group

## 2. Frontend Refactoring - Phase 2

### 2.1 Utility Modules
- [x] 2.1.1 Create bodystack.client/src/utils/date.ts with formatYearMonth function
- [x] 2.1.2 Create bodystack.client/src/utils/date.ts with isoDate function
- [x] 2.1.3 Create bodystack.client/src/api/errorHandling.ts with standardized error handling
- [x] 2.1.4 Create bodystack.client/src/styles/constants.ts with CARD_STYLES constant
- [x] 2.1.5 Create bodystack.client/src/styles/constants.ts with BUTTON_STYLES constant
- [x] 2.1.6 Update all existing components to use new utility modules

### 2.2 Component Splitting
- [x] 2.2.1 Extract IntegrationSelector component from DashboardShell
- [x] 2.2.2 Extract FitatuInlineLogin component from DashboardShell
- [x] 2.2.3 Extract SuuntoInlineLogin component from DashboardShell
- [x] 2.2.4 Update DashboardShell to use new child components
- [x] 2.2.5 Ensure proper prop drilling and callback passing
- [x] 2.2.6 Remove duplicated code between inline login components where possible

### 2.3 Code Cleanup
- [x] 2.3.1 Remove unused WeatherForecast endpoint from Program.cs
- [x] 2.3.2 Remove unused code from DashboardPage.tsx (commented sections)
- [x] 2.3.3 Consolidate date formatting in FitatuPage.tsx to use new utilities
- [x] 2.3.4 Update fitatuApi.ts to use standardized error handling
- [x] 2.3.5 Update suuntoApi.ts to use standardized error handling

## 3. Testing - Phase 3

### 3.1 Backend Unit Tests
- [x] 3.1.1 Create BodyStack.Server.Tests/Domain/Exceptions/DomainExceptionTests.cs
- [x] 3.1.2 Test FitatuSessionNotFoundException properties and inheritance
- [x] 3.1.3 Test MonthExportIncompleteException with missing days
- [x] 3.1.4 Create BodyStack.Server.Tests/Infrastructure/Persistence/FitatuSessionRepositoryTests.cs
- [x] 3.1.5 Test GetLatestAsync returns most recent session using InMemory database
- [x] 3.1.6 Test GetLatestAsync with empty database returns null
- [x] 3.1.7 Test GetLatestAsync with multiple sessions returns correct one

### 3.2 Backend Integration Tests
- [x] 3.2.1 Create BodyStack.Server.Tests/Integration/ExportEndpointTests.cs
- [x] 3.2.2 Test month export with valid session returns CSV
- [x] 3.2.3 Test month export without session returns 401
- [x] 3.2.4 Test month export with incomplete data returns 400 with error details
- [x] 3.2.5 Create BodyStack.Server.Tests/Integration/SignalRHubTests.cs
- [x] 3.2.6 Test hub connection and group joining
- [x] 3.2.7 Test progress messages are received only by correct user

### 3.3 Frontend Test Setup
- [x] 3.3.1 Configure Vitest in bodystack.client (vitest.config.ts)
- [x] 3.3.2 Add React Testing Library dependencies
- [x] 3.3.3 Setup test utilities and render helpers
- [x] 3.3.4 Create first component test (IntegrationSelector.test.tsx)
- [x] 3.3.5 Setup coverage reporting (vitest --coverage)

### 3.4 Frontend Tests
- [x] 3.4.1 Test date utility functions (formatYearMonth edge cases)
- [x] 3.4.2 Test error handling utilities
- [x] 3.4.3 Test IntegrationSelector renders all integration tabs
- [x] 3.4.4 Test IntegrationSelector calls onIntegrationChange when tab clicked
- [x] 3.4.5 Test FitatuInlineLogin form validation
- [x] 3.4.6 Test SuuntoInlineLogin form validation

## 4. Documentation & Verification

### 4.1 Documentation
- [x] 4.1.1 Document exception hierarchy in README or docs/
- [x] 4.1.2 Add inline comments explaining SignalR group usage
- [x] 4.1.3 Document query optimization patterns for future developers
- [x] 4.1.4 Update component documentation with new structure

### 4.2 Verification
- [x] 4.2.1 Run all existing tests - verify no regressions
- [ ] 4.2.2 Manual test Fitatu integration end-to-end (manual - requires API credentials)
- [ ] 4.2.3 Manual test Suunto integration end-to-end (manual - requires API credentials)
- [ ] 4.2.4 Test with multiple users to verify SignalR isolation (manual - requires running app)
- [x] 4.2.5 Verify error responses match standardized format
- [ ] 4.2.6 Check query performance with EXPLAIN ANALYZE (manual - requires database access)
- [x] 4.2.7 Review code coverage reports

### 4.3 Deployment Preparation
- [x] 4.3.1 Create deployment checklist
- [x] 4.3.2 Prepare rollback plan
- [x] 4.3.3 Set up monitoring for query performance
- [x] 4.3.4 Set up monitoring for SignalR connection errors
- [x] 4.3.5 Document any manual steps required
