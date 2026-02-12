## ADDED Requirements

### Requirement: Custom Exception Hierarchy
The system SHALL use custom exception types for domain-specific error conditions instead of string message comparison.

#### Scenario: Session Not Found Exception
- **WHEN** a user attempts to access Fitatu data without an active session
- **THEN** the system SHALL throw FitatuSessionNotFoundException
- **AND** the exception SHALL have ErrorCode "FITATU_SESSION_NOT_FOUND"
- **AND** the API SHALL return HTTP 401 with standardized error response

#### Scenario: Month Export Incomplete Exception
- **WHEN** a user attempts to export a month with incomplete data
- **THEN** the system SHALL throw MonthExportIncompleteException
- **AND** the exception SHALL include a list of missing days
- **AND** the API SHALL return HTTP 400 with error details

#### Scenario: Unauthorized Integration Exception
- **WHEN** an integration API returns 401/403
- **THEN** the system SHALL throw UnauthorizedIntegrationException
- **AND** the frontend SHALL be able to catch this specifically to trigger re-authentication

### Requirement: Exception Message Stability
Custom exceptions SHALL maintain backward-compatible message formats to prevent breaking existing catch blocks that may rely on message content.

#### Scenario: Backward Compatible Messages
- **WHEN** custom exceptions are thrown
- **THEN** the Message property SHALL contain the original text that was used in string comparisons
- **AND** new code SHOULD use exception type checking instead of message comparison

### Requirement: Standardized Error Responses
The API SHALL return consistent error response format for all exceptions.

#### Scenario: Error Response Format
- **WHEN** any exception is thrown during API request processing
- **THEN** the response SHALL be JSON with structure: `{ "error": { "code": "ERROR_CODE", "message": "User-friendly message" } }`
- **AND** stack traces SHALL NOT be included in production environment
- **AND** HTTP status codes SHALL be appropriate (400, 401, 403, 404, 500)

#### Scenario: Production Error Masking
- **WHEN** an unexpected exception occurs in production
- **THEN** the response SHALL show generic error message
- **AND** detailed error information SHALL be logged server-side
- **AND** error code SHALL be "INTERNAL_ERROR" for unhandled exceptions
