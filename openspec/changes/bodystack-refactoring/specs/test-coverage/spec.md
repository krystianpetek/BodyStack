## ADDED Requirements

### Requirement: Repository Test Coverage
Repository methods SHALL have unit tests using in-memory database.

#### Scenario: Latest Session Query Test
- **WHEN** testing FitatuSessionRepository.GetLatestAsync()
- **THEN** tests SHALL use EF Core InMemory provider
- **AND** tests SHALL verify only one record is returned
- **AND** tests SHALL verify correct ordering by UpdatedAt

#### Scenario: Empty Database Test
- **WHEN** repository methods are called on empty database
- **THEN** tests SHALL verify graceful handling (null/empty results)
- **AND** no exceptions SHALL be thrown for empty results

### Requirement: Exception Testing
Custom exceptions SHALL have tests verifying their behavior.

#### Scenario: Exception Properties Test
- **WHEN** custom exceptions are instantiated
- **THEN** tests SHALL verify ErrorCode is set correctly
- **AND** tests SHALL verify Message format is backward compatible

#### Scenario: Exception Handling Test
- **WHEN** exceptions are thrown in use cases
- **THEN** tests SHALL verify they are caught by correct catch blocks
- **AND** tests SHALL verify error responses are formatted correctly

### Requirement: Integration Tests
Critical API endpoints SHALL have integration tests.

#### Scenario: Export Endpoint Test
- **WHEN** testing month export endpoint
- **THEN** tests SHALL use TestServer with real database
- **AND** tests SHALL verify complete request/response cycle
- **AND** tests SHALL verify error scenarios (no session, incomplete month)

#### Scenario: SignalR Integration Test
- **WHEN** testing SignalR hub
- **THEN** tests SHALL verify group joining
- **AND** tests SHALL verify messages are sent to correct groups only

### Requirement: Frontend Test Setup
Frontend testing infrastructure SHALL be configured.

#### Scenario: Vitest Configuration
- **WHEN** frontend tests are run
- **THEN** Vitest SHALL be configured with React Testing Library
- **AND** tests SHALL support TypeScript
- **AND** coverage reporting SHALL be enabled

#### Scenario: Component Rendering Test
- **WHEN** testing React components
- **THEN** components SHALL render without errors
- **AND** component props SHALL be properly typed
- **AND** async operations SHALL be properly handled in tests

### Requirement: Test Organization
Tests SHALL follow consistent organization and naming.

#### Scenario: Test File Naming
- **WHEN** creating test files
- **THEN** they SHALL follow pattern `[ClassName]Tests.cs` for backend
- **AND** `[ComponentName].test.tsx` for frontend
- **AND** test files SHALL mirror structure of source files

#### Scenario: Test Method Naming
- **WHEN** naming test methods
- **THEN** they SHALL be descriptive (e.g., `GetLatestAsync_WithMultipleSessions_ReturnsMostRecent`)
- **AND** they SHALL follow pattern `MethodName_Scenario_ExpectedResult`
