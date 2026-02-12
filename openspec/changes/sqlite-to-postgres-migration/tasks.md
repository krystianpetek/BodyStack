## 1. Docker i PostgreSQL

- [x] 1.1 Utworzyć docker-compose.yml z PostgreSQL na porcie 12334
- [x] 1.2 Skonfigurować bazę danych "bodystack", użytkownika "bodystack_user", hasło "bodystack_pass"

## 2. Zmiana Dependencies

- [x] 2.1 Usunąć Microsoft.EntityFrameworkCore.Sqlite z BodyStack.Server.csproj
- [x] 2.2 Dodać Npgsql.EntityFrameworkCore.PostgreSQL w wersji 10.0.0

## 3. Konfiguracja Connection String

- [x] 3.1 Zmienić connection string w appsettings.json na PostgreSQL
- [x] 3.2 Użyć: Host=localhost;Port=12334;Database=bodystack;Username=bodystack_user;Password=bodystack_pass

## 4. Aktualizacja Kodu

- [x] 4.1 Zmienić UseSqlite na UseNpgsql w Program.cs
- [x] 4.2 Dodać HasPrecision(18, 2) dla pól Energy, Protein, Fat, Carbohydrate, Fiber, Sugars, Salt w AppDbContext

## 5. Cleanup SQLite

- [x] 5.1 Usunąć plik bodystack.db z root repozytorium
- [x] 5.2 Sprawdzić czy nie ma innych referencji do SQLite w kodzie

## 6. Testowanie

- [x] 6.1 Uruchomić docker-compose up dla PostgreSQL
- [x] 6.2 Uruchomić aplikację i zweryfikować że EF Core tworzy tabele
- [x] 6.3 Sprawdzić czy API działa poprawnie (testowy request)
