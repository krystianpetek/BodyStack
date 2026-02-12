## Context

Obecnie aplikacja BodyStack używa SQLite jako bazy danych - plik `bodystack.db` w root repozytorium. Entity Framework Core 10 jest skonfigurowany z providerem SQLite. Wymagana jest migracja na PostgreSQL ze względu na lepszą skalowalność i kompatybilność z produkcyjnymi deploymentami.

PostgreSQL będzie uruchomiony lokalnie w Dockerze na porcie 12334.

## Goals / Non-Goals

**Goals:**
- Zamiana providera EF Core z SQLite na PostgreSQL (Npgsql)
- Konfiguracja PostgreSQL w Dockerze na localhost:12334
- Zachowanie identycznej funkcjonalności aplikacji
- Usunięcie wszystkich śladów SQLite z projektu
- Dodanie precyzji dla pól decimal (wymaganie PostgreSQL)

**Non-Goals:**
- Migracja danych ze starej bazy SQLite (start od zera)
- Zmiana schematu bazy danych (poza precyzją decimal)
- Modyfikacja logiki biznesowej
- Wprowadzanie nowych funkcjonalności

## Decisions

**1. Npgsql.EntityFrameworkCore.PostgreSQL v10.0.0**
- Wybrano wersję 10.0.0 która jest kompatybilna z EF Core 10
- Alternatywa: użycie wersji 9.x - odrzucono, lepiej być na bieżąco z główną wersją EF Core

**2. Docker Compose dla PostgreSQL**
- Wybrano docker-compose zamiast manualnego uruchamiania kontenera
- Ułatwia zarządzanie konfiguracją i pozwala na łatwe dodanie wolumenów w przyszłości
- Port 12334: unikatowy, nie koliduje z domyślnym 5432

**3. HasPrecision(18,2) dla pól decimal**
- PostgreSQL wymaga jawnego określenia precyzji dla typu decimal
- 18 cyfr całkowitych, 2 po przecinku - wystarczające dla wartości odżywczych
- Alternatywa: decimal(10,2) - odrzucono, 18 daje większy zakres na przyszłość

**4. Brak migracji danych**
- Użytkownik wyraźnie zaznaczył że nie potrzebuje migrować danych
- Prostsze rozwiązanie - start z czystą bazą

## Risks / Trade-offs

- **[Risk]** Wymagany Docker do uruchomienia aplikacji → **Mitigation**: Dodanie dokumentacji setupu
- **[Risk]** Zmiana zachowania przy zaokrągleniach decimal → **Mitigation**: Testy integracyjne
- **[Risk]** Connection string z hasłem w appsettings.json → **Mitigation**: W dev używamy lokalnego postgres, w prod będzie zmienna środowiskowa

## Migration Plan

1. Zatrzymać aplikację
2. Utworzyć docker-compose.yml i uruchomić PostgreSQL
3. Zmienić zależności w .csproj
4. Zaktualizować connection string
5. Zmienić UseSqlite na UseNpgsql w Program.cs
6. Dodać HasPrecision w AppDbContext
7. Uruchomić aplikację - EF Core utworzy schemat automatycznie (EnsureCreated)
8. Usunąć bodystack.db

## Open Questions

- Brak
