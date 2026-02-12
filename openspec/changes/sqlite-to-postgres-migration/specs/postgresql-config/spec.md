## ADDED Requirements

### Requirement: PostgreSQL dostępny w Dockerze
System SHALL zapewnić PostgreSQL uruchomionego w Dockerze na localhost:12334.

#### Scenario: Kontener PostgreSQL działa
- **WHEN** docker-compose up zostanie wykonane
- **THEN** PostgreSQL nasłuchuje na localhost:12334
- **AND** baza danych "bodystack" jest dostępna
- **AND** użytkownik "bodystack_user" ma uprawnienia do operacji CRUD

### Requirement: Connection string dla PostgreSQL
System SHALL używać connection string wskazującego na PostgreSQL zamiast SQLite.

#### Scenario: Aplikacja łączy się z PostgreSQL
- **WHEN** aplikacja startuje
- **THEN** EF Core łączy się z PostgreSQL na localhost:12334
- **AND** używa bazy danych "bodystack"
- **AND** uwierzytelnianie odbywa się jako "bodystack_user"
