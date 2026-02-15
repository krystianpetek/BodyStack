## Why

Obecne integracje z Fitatu i Suunto API używają standardowego `HttpClient` z synchronicznym/asynchronicznym patternem. Dla dużych zakresów dat (np. pobieranie danych za cały rok) aplikacja musi czekać na pełne załadowanie wszystkich danych do pamięci. RX.NET umożliwi strumieniowe przetwarzanie danych (streaming), gdzie dane są przetwarzane "w locie" bez konieczności trzymania wszystkiego w pamięci. To zmniejsza zużycie RAM i pozwala na wcześniejsze rozpoczęcie przetwarzania (np. pokazywanie partial results).

## What Changes

- Implementacja strumieniowych HTTP responses przy użyciu `Observable<HttpResponseMessage>`
- Przetwarzanie JSON w streamie (JsonSerializer.DeserializeAsyncEnumerable + RX)
- Backpressure handling dla dużych datasetów (Buffer, Sample, Throttle)
- Retry policies z exponential backoff dla requestów HTTP
- Circuit breaker dla ochrony przed przeciążeniem API zewnętrznych
- Rate limiting zgodny z limitami API Fitatu/Suunto
- Eager cancellation - możliwość anulowania długich operacji przez użytkownika
- Progress reporting dla długich operacji pobierania

## Capabilities

### New Capabilities

- `streamed-http-client`: Strumieniowy HttpClient z RX.NET dla dużych response'ów
- `json-streaming-rx`: Strumieniowe parsowanie JSON z Observable<T>
- `backpressure-handling`: Obsługa backpressure dla dużych datasetów
- `api-resilience-patterns`: Retry, circuit breaker, rate limiting dla API calls

### Modified Capabilities

- `fitatu-api-integration`: Refaktoryzacja na strumieniowe pobieranie (nie zmienia API publicznego)
- `suunto-api-integration`: Refaktoryzacja na strumieniowe pobieranie (nie zmienia API publicznego)

## Impact

### Affected Code
- `BodyStack.Server/Integrations/Fitatu/FitatuClient.cs` - refactor na strumieniowe API
- `BodyStack.Server/Integrations/Fitatu/IFitatuClient.cs` - zmiany w interface (zwraca Observable)
- `BodyStack.Server/Integrations/Suunto/SuuntoActivityExportClient.cs` - refactor na streaming
- `BodyStack.Server/Integrations/Suunto/SuuntoSleepExportClient.cs` - refactor na streaming
- `BodyStack.Server/Application/Fitatu/*UseCase.cs` - adaptacja do Observable responses
- `BodyStack.Server/Application/Suunto/*UseCase.cs` - adaptacja do Observable responses
- `BodyStack.Server/Infrastructure/Http/` - nowy katalog dla RX HTTP extensions

### APIs
- Brak zmian w publicznych endpointach HTTP API
- Internal changes w return types (Task<T> → IObservable<T> lub IAsyncEnumerable<T>)
- API endpoints mogą oferować partial results przez SignalR

### Dependencies
- `System.Reactive` (już dodane)
- `System.Net.Http.Json` - dla streamingu JSON (jeśli nie jest używane)
- `Polly` (opcjonalnie) - dla advanced retry/circuit breaker

### Systems
- Fitatu Integration - strumieniowe pobieranie danych
- Suunto Integration - strumieniowe pobieranie danych
- Memory Management - znaczące zmniejszenie zużycia RAM dla dużych zakresów
- User Experience - wcześniejsze wyświetlanie partial results

### Testing
- Nowe testy dla strumieniowego HTTP clienta
- Testy backpressure (symulacja dużych datasetów)
- Testy retry policies i circuit breaker
- Testy wydajnościowe (porównanie RAM: streaming vs buffering)
- Testy cancellation (symulacja anulowania przez użytkownika)

### Performance Impact
- **RAM**: Zmniejszenie zużycia pamięci o ~70-90% dla dużych datasetów
- **Latency**: Szybszy time-to-first-result (pierwsze dane dostępne od razu)
- **Throughput**: Potencjalnie wolniejszy total time, ale lepsze UX

### Breaking Changes
- **BREAKING**: Internal interfaces IFitatuClient, ISuuntoActivityExportClient zmieniają return types
- **BREAKING**: Use case'y muszą obsługiwać Observable zamiast Task
- Nie ma breaking changes w publicznym HTTP API dla frontendu

### Risks
- **Complexity**: RX.NET + streaming JSON to zaawansowane techniki
- **Debugging**: Trudniejsze debugowanie strumieni asynchronicznych
- **Backwards compatibility**: Upewnić się, że istniejące flow nadal działa
