## ADDED Requirements

### Requirement: Component Separation
DashboardShell component SHALL be split into smaller, focused components with single responsibilities.

#### Scenario: Integration Selector Component
- **WHEN** the Dashboard renders
- **THEN** IntegrationSelector SHALL handle integration tab switching
- **AND** it SHALL receive available integrations as props
- **AND** it SHALL call onIntegrationChange when user switches tabs

#### Scenario: Inline Login Components
- **WHEN** user needs to authenticate with an integration
- **THEN** FitatuInlineLogin and SuuntoInlineLogin SHALL handle respective login flows
- **AND** each SHALL manage its own form state and validation
- **AND** on successful login, they SHALL notify parent component

#### Scenario: DashboardShell Orchestrator
- **WHEN** DashboardShell mounts
- **THEN** it SHALL coordinate between child components
- **AND** it SHALL maintain routing and layout
- **AND** it SHALL pass necessary callbacks to children

### Requirement: Utility Module Creation
Common logic SHALL be extracted into reusable utility modules.

#### Scenario: Date Formatting Utility
- **WHEN** dates need to be formatted as "yyyy-MM"
- **THEN** formatYearMonth(date: Date) utility SHALL be used
- **AND** it SHALL handle all edge cases (month padding, timezone)

#### Scenario: Error Handling Utility
- **WHEN** API calls fail
- **THEN** standardized error handling utilities SHALL be used
- **AND** all API clients SHALL use consistent error handling pattern

#### Scenario: UI Constants
- **WHEN** Tailwind classes are repeated across components
- **THEN** they SHALL be extracted to constants (CARD_STYLES, BUTTON_STYLES)
- **AND** components SHALL import and use these constants

### Requirement: Component File Organization
Refactored components SHALL follow consistent file organization.

#### Scenario: Component Directory Structure
- **WHEN** new components are created
- **THEN** they SHALL be placed in appropriate directories
- **AND** related components SHALL be co-located (e.g., inline logins together)
- **AND** each component SHALL have its own file
