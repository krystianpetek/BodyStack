## MODIFIED Requirements

### Requirement: Entity Framework Core z PostgreSQL
System SHALL używać Npgsql.EntityFrameworkCore.PostgreSQL zamiast Microsoft.EntityFrameworkCore.Sqlite.

#### Scenario: Provider PostgreSQL jest skonfigurowany
- **WHEN** aplikacja konfiguruje DbContext
- **THEN** używa metody UseNpgsql zamiast UseSqlite
- **AND** używa connection string "Default" z PostgreSQL

### Requirement: Precyzja typów decimal
System SHALL definiować precyzję dla wszystkich pól typu decimal w encji MonthDaySummary.

#### Scenario: Encja MonthDaySummary ma precyzję decimal
- **WHEN** EF Core tworzy schemat bazy
- **THEN** pola Energy, Protein, Fat, Carbohydrate, Fiber, Sugars, Salt mają typ decimal(18,2)
- **AND** operacje zapisu/odczytu zachowują precyzję 2 miejsc po przecinku

## REMOVED Requirements

### Requirement: SQLite jako baza danych
**Reason**: Zastąpiony przez PostgreSQL dla lepszej skalowalności
**Migration**: Użyj nowego connection string w appsettings.json, uruchom PostgreSQL w Dockerze
