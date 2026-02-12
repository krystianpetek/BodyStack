## ADDED Requirements

### Requirement: Efficient Repository Queries
Repository methods SHALL use database-level sorting and filtering instead of loading entire datasets into memory.

#### Scenario: Latest Session Query Optimization
- **WHEN** FitatuSessionRepository.GetLatestAsync() is called
- **THEN** the query SHALL use `OrderByDescending(x => x.UpdatedAt).FirstOrDefaultAsync()`
- **AND** the query SHALL NOT use `ToListAsync()` before filtering
- **AND** the database SHALL perform the sorting and limiting

#### Scenario: Memory Efficiency
- **WHEN** the FitatuSessions table contains 10,000+ records
- **THEN** GetLatestAsync SHALL load only 1 record into memory
- **AND** query execution time SHALL be under 50ms
- **AND** memory usage SHALL remain constant regardless of table size

### Requirement: Query Performance Monitoring
Repository queries SHOULD be monitored for performance degradation.

#### Scenario: Slow Query Detection
- **WHEN** a repository query takes longer than 100ms
- **THEN** the system SHALL log a warning with query details
- **AND** the log SHALL include the method name and execution time

#### Scenario: Query Plan Optimization
- **WHEN** repository queries are executed frequently
- **THEN** appropriate database indexes SHALL be in place
- **AND** query execution plans SHALL be reviewed periodically
