# BodyStack Exception Hierarchy

This document describes the custom exception hierarchy used in the BodyStack application.

## Overview

All domain exceptions inherit from the base `DomainException` class, which provides a standardized way to handle errors with error codes. This approach replaces fragile string-based exception message comparisons.

## Exception Types

### Base Exception

#### `DomainException` (abstract)
The base class for all domain exceptions.

**Properties:**
- `ErrorCode: string` - A machine-readable error code (e.g., "FITATU_SESSION_NOT_FOUND")

**Usage:**
```csharp
// Never instantiate directly - use derived classes
throw new FitatuSessionNotFoundException("user123");
```

### Specific Exceptions

#### `FitatuSessionNotFoundException`
Thrown when a Fitatu session cannot be found for a user.

**Error Code:** `FITATU_SESSION_NOT_FOUND`

**Properties:**
- `FitatuUserId: string?` - The Fitatu user ID that was not found

**When to use:**
```csharp
var session = await _sessionRepository.GetLatestAsync();
if (session is null)
{
    throw new FitatuSessionNotFoundException(null);
}
```

#### `MonthExportIncompleteException`
Thrown when attempting to export a month that has incomplete computed days.

**Error Code:** `MONTH_EXPORT_INCOMPLETE`

**Properties:**
- `Year: int` - The year of the incomplete export
- `Month: int` - The month of the incomplete export
- `MissingDays: IReadOnlyList<string>` - List of dates that are missing (format: YYYY-MM-DD)

**When to use:**
```csharp
var missing = expectedDates.Where(d => !ready.ContainsKey(d)).ToList();
if (missing.Count > 0)
{
    throw new MonthExportIncompleteException(year, month, missing);
}
```

#### `UnauthorizedIntegrationException`
Thrown when an integration API returns 401 or 403.

**Error Code:** `UNAUTHORIZED_INTEGRATION`

**Properties:**
- `IntegrationName: string` - Name of the integration (e.g., "Fitatu", "Suunto")
- `StatusCode: int?` - The HTTP status code received

#### `IntegrationApiException`
Thrown for general API errors when calling external integrations.

**Error Code:** `INTEGRATION_API_ERROR`

**Properties:**
- `IntegrationName: string` - Name of the integration
- `StatusCode: int?` - The HTTP status code
- `ResponseBody: string?` - The raw response body (if available)

## Best Practices

1. **Always use specific exception types** instead of generic `Exception` or `InvalidOperationException`
2. **Include context in the message** but keep error codes stable
3. **Catch specific exceptions first** in exception handlers
4. **Don't catch DomainException directly** unless you need to handle all domain errors uniformly

## Error Response Format

When exceptions are caught in the API layer, they are converted to a standardized JSON response:

```json
{
  "error": {
    "code": "FITATU_SESSION_NOT_FOUND",
    "message": "User-friendly error message",
    "details": { /* optional context-specific data */ }
  }
}
```

## Migration Guide

### Before (Anti-pattern):
```csharp
catch (InvalidOperationException ex) when (ex.Message.StartsWith("Fitatu session not found"))
{
    return Results.Unauthorized();
}
```

### After (Pattern):
```csharp
catch (FitatuSessionNotFoundException)
{
    return Results.Unauthorized();
}
```

## Adding New Exceptions

To add a new exception type:

1. Create a new class inheriting from `DomainException`
2. Define a unique `ErrorCode` constant
3. Add relevant properties for context
4. Update this documentation
5. Add corresponding tests in `DomainExceptionTests.cs`
