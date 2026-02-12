## Context

### Current State
BodyStack is a personal health dashboard built with .NET 10 backend and React 19 frontend. The application integrates with external APIs (Fitatu for nutrition, Suunto for fitness) and uses SignalR for real-time progress updates during background processing.

**Critical Issues Identified:**
1. **Exception Handling Anti-Pattern**: Using string comparison on exception messages for control flow (e.g., `ex.Message.StartsWith("Fitatu session not found")`)
2. **SignalR Privacy Leak**: Broadcasting progress to ALL connected clients instead of isolating by user
3. **Inefficient Database Queries**: Loading entire tables into memory before filtering (e.g., `ToListAsync().OrderBy().FirstOrDefault()`)
4. **Minimal Test Coverage**: Only 3 test methods for entire codebase
5. **Monolithic Components**: DashboardShell is 224 lines with multiple responsibilities

### Architecture
- **Backend**: Clean Architecture lite with Domain, Application, Infrastructure, Integrations layers
- **Frontend**: React with hooks, context providers, custom components
- **Database**: PostgreSQL with EF Core 10
- **Real-time**: SignalR hubs for background job progress

### Constraints
- Must maintain backward compatibility
- Zero breaking changes to API contracts
- No database schema changes
- Must work with existing integrations (Fitatu, Suunto)

## Goals / Non-Goals

**Goals:**
- Eliminate fragile exception message comparisons
- Isolate SignalR communication per user (security/privacy)
- Optimize database queries for performance
- Improve code maintainability through component splitting
- Establish testing foundation with critical path coverage
- Standardize error handling across backend and frontend

**Non-Goals:**
- Migrating to full Clean Architecture or different patterns
- Changing database provider or schema
- Adding new features or integrations
- Complete rewrite of any module
- Changing authentication mechanism

## Decisions

### 1. Custom Exception Hierarchy
**Decision**: Create domain-specific exception types inheriting from base `DomainException`

**Rationale**: 
- String comparison is fragile and breaks if messages change
- Custom types enable proper exception filtering without magic strings
- Better testability - can catch specific exception types
- Clearer intent in code

**Structure**:
```
DomainException (abstract)
├── FitatuSessionNotFoundException
├── MonthExportIncompleteException  
├── UnauthorizedIntegrationException
└── IntegrationApiException
```

**Alternatives Considered**:
- Result pattern (Railway-oriented programming) - rejected due to scope complexity
- Error codes in exceptions - accepted as complement, not replacement

### 2. SignalR Groups Instead of Broadcast
**Decision**: Use SignalR Groups with user-specific group names (`user-{userId}`)

**Rationale**:
- `Clients.All` sends progress to every connected user (privacy violation)
- Groups allow targeted messaging to specific users
- Minimal code change - just replace `Clients.All` with `Clients.Group`
- Frontend already has user context from JWT

**Implementation**:
```csharp
// Backend
await _hub.Clients.Group($"user-{fitatuUserId}").SendAsync("Progress", progress);

// Frontend - on connection
connection.invoke("JoinUserGroup", userId);
```

**Alternatives Considered**:
- User-specific hub instances - too complex
- Client-side filtering - still sends data to wrong clients

### 3. Repository Query Optimization
**Decision**: Use `OrderBy().FirstOrDefaultAsync()` instead of `ToList().OrderBy().FirstOrDefault()`

**Rationale**:
- Current code loads ALL records then sorts in memory (O(n) memory)
- Database can sort and limit much more efficiently (O(1) memory)
- Single query instead of fetch + process
- Critical for tables that will grow over time

**Query Change**:
```csharp
// BEFORE (inefficient)
var entities = await _db.FitatuSessions.AsNoTracking().ToListAsync();
var entity = entities.OrderByDescending(x => x.UpdatedAt).FirstOrDefault();

// AFTER (efficient)
var entity = await _db.FitatuSessions
    .OrderByDescending(x => x.UpdatedAt)
    .FirstOrDefaultAsync();
```

### 4. Component Splitting Strategy
**Decision**: Split DashboardShell into 3 focused components

**Rationale**:
- 224 lines is too much for single component
- Multiple responsibilities: routing, auth, integration selection, inline logins
- Splitting improves testability and reusability

**New Structure**:
```
DashboardShell/
├── DashboardShell.tsx (orchestrator, ~80 lines)
├── IntegrationSelector.tsx (tabs, ~60 lines)
├── FitatuInlineLogin.tsx (login form, ~50 lines)
└── SuuntoInlineLogin.tsx (login form, ~50 lines)
```

### 5. Testing Approach
**Decision**: Focus on critical paths first, use in-memory EF Core for repository tests

**Rationale**:
- Full test coverage is unrealistic for refactoring scope
- Focus on areas that broke in the past or are critical
- In-memory EF Core is fast and doesn't require Docker
- Integration tests with TestContainers for critical endpoints

**Priority Order**:
1. Repository queries (data access layer)
2. Exception handling (new code)
3. API endpoints (integration tests)
4. SignalR hub (connectivity)

### 6. Error Response Standardization
**Decision**: Create consistent error response format with error codes

**Format**:
```json
{
  "error": {
    "code": "FITATU_SESSION_NOT_FOUND",
    "message": "User-friendly message",
    "details": { /* optional context */ }
  }
}
```

**Rationale**:
- Frontend can handle errors programmatically by code
- No stack traces leak to production clients
- Consistent UX across all endpoints

## Risks / Trade-offs

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| SignalR changes break real-time updates | Medium | High | Thorough testing with multiple concurrent users; feature flag for rollback |
| Exception changes break existing catch blocks | Low | Medium | Search codebase for all exception handling; maintain backward compatible messages |
| Query optimization changes behavior | Low | High | Add comprehensive tests for edge cases (empty results, nulls, ties) |
| Component splitting breaks routing | Low | Medium | Test all navigation paths; verify lazy loading still works |
| Test environment differences | Medium | Medium | Use in-memory DB that matches production behavior; add integration tests |

### Known Trade-offs
1. **Custom Exceptions vs Result Pattern**: Exceptions have performance cost for control flow, but Result pattern would require major refactoring. Chose exceptions for pragmatic approach.

2. **In-Memory vs Real Database Tests**: In-memory is fast but may behave differently. Compromise: unit tests with in-memory, integration tests with real database.

3. **Component Splitting Overhead**: More files to maintain, but better organization. Acceptable trade-off for maintainability.

## Migration Plan

### Phase 1: Backend Critical (Deploy First)
1. Create exception types
2. Update repository queries
3. Implement SignalR groups
4. Update Program.cs exception handling
5. Deploy and monitor

### Phase 2: Frontend & Testing
1. Create utility modules
2. Split components
3. Add tests
4. Deploy

### Rollback Strategy
- Each phase is independently deployable
- Database schema unchanged - no rollback needed there
- If SignalR issues: quick fix to revert to broadcast mode
- Feature flags for risky changes (optional)

## Open Questions

1. **Indexing**: Should we add database index on `FitatuSessions.UpdatedAt`? 
   - *Research needed*: Check query execution plan

2. **SignalR Group Management**: Should we auto-cleanup groups on disconnect?
   - *Assumption*: SignalR handles this automatically

3. **Test Coverage Target**: What's minimum acceptable coverage?
   - *Proposal*: 60% for new code, critical paths for existing

4. **Error Code Format**: snake_case or PascalCase?
   - *Proposal*: UPPER_SNAKE_CASE for consistency

## Security Considerations

### Threat Mitigations
1. **SignalR Broadcast** → **FIXED**: Groups isolate user data
2. **Token Decryption** → **PARTIAL**: Optimized to decrypt only needed tokens, but still in memory
3. **Error Information Leak** → **FIXED**: Standardized responses without stack traces

### Compliance
- GDPR: User data isolation satisfied
- Security by Design: Defense in depth with multiple layers
