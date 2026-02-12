## Why

SQLite jest wystarczające dla lokalnego developmentu, ale PostgreSQL oferuje lepszą wydajność przy wielu równoczesnych operacjach, wsparcie dla zaawansowanych typów danych oraz lepszą skalowalność. Przejście na PostgreSQL w Dockerze ułatwi późniejsze wdrożenie na produkcję i zapewni spójne środowisko deweloperskie.

## What Changes

- **BREAKING**: Usunięcie SQLite i wszystkich jego zależności
- **BREAKING**: Zmiana connection string na PostgreSQL
- Aktualizacja Entity Framework Core providera z SQLite na PostgreSQL (Npgsql)
- Dodanie precyzji dla pól `decimal` w schemacie bazy danych (wymagane przez PostgreSQL)
- Utworzenie `docker-compose.yml` z PostgreSQL na porcie 12334
- Usunięcie pliku `bodystack.db` i wszystkich referencji do SQLite
- Aktualizacja dokumentacji setupu deweloperskiego

## Capabilities

### New Capabilities
- `postgresql-config`: Konfiguracja PostgreSQL w Dockerze, zmienne środowiskowe, connection string

### Modified Capabilities
- `database-persistence`: Zmiana implementacji persistence z SQLite na PostgreSQL, dodanie precyzji dla decimal

## Impact

- **Backend**: `BodyStack.Server.csproj` - zmiana pakietu EF Core
- **Backend**: `Program.cs` - zmiana `UseSqlite` na `UseNpgsql`
- **Backend**: `appsettings.json` - nowy connection string PostgreSQL
- **Backend**: `AppDbContext.cs` - dodanie `.HasPrecision()` dla pól decimal w encji MonthDaySummary
- **Root**: Nowy plik `docker-compose.yml` z PostgreSQL
- **Root**: Usunięcie `bodystack.db`
- **DevEx**: Wymagany Docker do uruchomienia bazy danych
