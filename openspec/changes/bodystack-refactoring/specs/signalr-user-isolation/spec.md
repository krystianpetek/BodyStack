## ADDED Requirements

### Requirement: SignalR User Isolation
The system SHALL isolate SignalR communication so users only receive their own progress updates.

#### Scenario: Group-Based Messaging
- **WHEN** a background job sends progress updates via SignalR
- **THEN** the system SHALL use `Clients.Group($"user-{userId}")` instead of `Clients.All`
- **AND** only the user who initiated the operation SHALL receive the progress

#### Scenario: Group Join on Connection
- **WHEN** a client connects to the SignalR hub
- **THEN** the client SHALL automatically join a group named after their user ID
- **AND** the group name SHALL be formatted as "user-{userId}"

#### Scenario: Multi-User Isolation
- **WHEN** two different users simultaneously perform month recalculation
- **THEN** each user SHALL only see their own progress updates
- **AND** user A SHALL NOT see user B's progress
- **AND** no cross-contamination of data SHALL occur

### Requirement: Connection Resilience
The system SHALL handle SignalR connection interruptions gracefully.

#### Scenario: Reconnection Group Rejoin
- **WHEN** a client's SignalR connection drops and reconnects
- **THEN** the client SHALL automatically rejoin their user group
- **AND** any in-progress operations SHALL continue sending updates to the correct group

#### Scenario: Disconnection Cleanup
- **WHEN** a client disconnects from SignalR
- **THEN** the system SHALL NOT remove the user group (other connections may exist)
- **AND** SignalR SHALL handle group cleanup automatically when no connections remain
