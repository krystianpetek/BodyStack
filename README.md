# BodyStack

Monorepo dla aplikacji **BodyStack** (backend .NET + frontend React). Projekt jest przygotowany pod wiele integracji (np. **Fitatu**, **Suunto** oraz scaffold **Template**) i posiada publiczny **landing page** z i18n oraz globalnym przełączaniem motywu.

## Stack

- **Backend**: .NET (minimal API), EF Core + PostgreSQL
- **Frontend**: React 19 + TypeScript, Vite, Tailwind CSS v4, React Router
- **Realtime**: SignalR (live updates postępu przeliczeń)
- **i18n**: i18next (EN/PL)
- **Theme**: light/dark (globalny `ThemeProvider`, zapis w `localStorage`)

## Struktura repo

- `BodyStack.Server/`
  - API + SignalR hub
  - PostgreSQL (Docker, localhost:12334)
- `bodystack.client/`
  - aplikacja SPA (dashboard + landing)

## Uruchomienie (dev)

### Wymagania

- Node.js (zalecane LTS)
- .NET SDK

### Backend

Uruchom backend z poziomu IDE lub CLI.

Przykładowo (z root repo):

```bash
# Windows / PowerShell
# (uruchom w folderze BodyStack.Server)

dotnet run
```

Backend domyślnie wystawia endpointy pod `/api/*` oraz SignalR hub (wykorzystywany przez Fitatu dashboard).

### Frontend

W osobnym terminalu:

```bash
# w folderze bodystack.client
npm install
npm run dev
```

## Routing / Widoki

- `/` – landing page (publiczny)
- `/dashboard` – shell dashboardu z wyborem integracji (nawigacja i topbar zawsze widoczne)
- `/dashboard/fitatu` – integracja Fitatu
- `/dashboard/suunto` – integracja Suunto
- `/dashboard/template` – integracja Template (scaffold / placeholder)

## Integracje

Aplikacja jest projektowana pod wiele integracji, a UX jest spójny:
- dashboard + sidebar są stale widoczne,
- logowanie/stan połączenia jest obsługiwany inline wewnątrz danej integracji.

### Fitatu

- logowanie odbywa się z poziomu dashboardu (inline),
- dostępne są widoki kalendarza/statusów dni, szczegóły dnia, eksport CSV,
- backend posiada endpoint wylogowania (czyszczenie sesji).

### Suunto

- integracja w trakcie rozbudowy,
- placeholder flow po stronie UI.

### Template

Scaffold do szybkiego dodawania kolejnych integracji.
Plik startowy: `bodystack.client/src/pages/template/TemplatePage.tsx`.

## i18n (EN/PL)

- inicjalizacja: `bodystack.client/src/i18n.ts`
- język jest zapisywany w `localStorage` pod kluczem `bodystack.language`
- przełącznik języka jest dostępny:
  - na landing page (header)
  - w dashboardzie (TopBar)

## Theme (light/dark)

- provider: `bodystack.client/src/providers/ThemeProvider.tsx`
- hook: `bodystack.client/src/hooks/useTheme.ts`
- motyw jest zapisywany w `localStorage` pod kluczem `bodystack.theme`

## Build

Frontend:

```bash
# w folderze bodystack.client
npm run build
npm run preview
```

Backend:

```bash
# w folderze BodyStack.Server
dotnet build
```

## Baza danych PostgreSQL

Aplikacja wymaga PostgreSQL uruchomionego lokalnie w Dockerze:

```bash
# Uruchomienie bazy
docker-compose up -d

# PostgreSQL dostępny na localhost:12334
# Baza: bodystack
# Użytkownik: bodystack_user
# Hasło: bodystack_pass
```

## Licencja

Brak (do uzupełnienia).
