## 1. Setup and Dependencies

- [x] 1.1 Dodać Polly (8.0.0) do BodyStack.Server.csproj
- [x] 1.2 Utworzyć katalog Infrastructure/Http/ dla RX HTTP components
- [x] 1.3 Utworzyć podkatalog Infrastructure/Http/Streaming/ dla streaming-specific code
- [x] 1.4 Utworzyć podkatalog Infrastructure/Http/Resilience/ dla Polly policies
- [x] 1.5 Dodać feature flag UseStreamingApi w appsettings.json

## 2. Reactive HTTP Client Infrastructure

- [ ] 2.1 Zaimplementować interface IReactiveHttpClient (NIE POTRZEBNE - uproszczone)
- [x] 2.2 Używać HttpCompletionOption.ResponseHeadersRead
- [ ] 2.3 Dodać extension method ToObservable() dla HttpResponseMessage (NIE POTRZEBNE)
- [ ] 2.4 Zaimplementować GetStreamAsyncObservable() zwracający IObservable<Stream> (NIE POTRZEBNE)
- [x] 2.5 Dodać error handling dla HTTP errors (OnError propagation)

## 3. JSON Streaming Components

- [ ] 3.1 Zaimplementować JsonStreamingSerializer<T> (NIE POTRZEBNE - uproszczone)
- [x] 3.2 Używać Observable.FromAsync() dla prostoty
- [x] 3.3 Zaimplementować stream cancellation support (CancellationToken propagation)
- [ ] 3.4 Dodać custom JsonSerializerOptions dla streaming (NIE POTRZEBNE)
- [ ] 3.5 Zaimplementować buffering dla Utf8JsonReader (NIE POTRZEBNE)

## 4. Backpressure Handling

- [ ] 4.1 Zaimplementować BackpressureBuffer<T> (NIE POTRZEBNE - uproszczone)
- [ ] 4.2 Dodać enum BackpressureStrategy (NIE POTRZEBNE)
- [ ] 4.3 Zaimplementować Sample operator (NIE POTRZEBNE)
- [ ] 4.4 Zaimplementować Throttle operator (NIE POTRZEBNE)
- [ ] 4.5 Dodać Batch operator (NIE POTRZEBNE)

## 5. Resilience Patterns with Polly

- [x] 5.1 Skonfigurować RetryPolicy z exponential backoff (1s, 2s, 4s)
- [x] 5.2 Dodać filter dla retryable exceptions (HttpRequestException, TimeoutException)
- [ ] 5.3 Skonfigurować CircuitBreakerPolicy (5 failures, 1 minute timeout) (NIE POTRZEBNE - uproszczone)
- [ ] 5.4 Zaimplementować custom CircuitBreakerOpenException (NIE POTRZEBNE)
- [ ] 5.5 Dodać RateLimitPolicy (X requests per second per API) (NIE POTRZEBNE)
- [x] 5.6 Zaimplementować TimeoutPolicy (default: 30s per request)
- [x] 5.7 Połączyć policies używając PolicyWrap

## 6. Fitatu Client Refactoring

- [x] 6.1 Refactor IFitatuClient - zmienić return type z Task<T> na IObservable<T>
- [x] 6.2 Uprościć FitatuClient - Observable.FromAsync z Polly
- [x] 6.3 Dodać cancellation support do wszystkich metod
- [x] 6.4 Zintegrować Polly policies z FitatuClient
- [ ] 6.5 Dodać progress reporting dla długich operacji (opcjonalnie)

## 7. Suunto Client Refactoring

- [x] 7.1 Refactor ISuuntoActivityExportClient na IObservable<T>
- [x] 7.2 Refactor ISuuntoSleepExportClient na IObservable<T>
- [x] 7.3 Uprościć SuuntoActivityExportClient - Observable.FromAsync z Polly
- [x] 7.4 Uprościć SuuntoSleepExportClient - Observable.FromAsync z Polly
- [x] 7.5 Dodać cancellation support do wszystkich metod
- [x] 7.6 Zintegrować Polly policies z Suunto clients

## 8. Use Case Adaptation

- [x] 8.1 Refactor FitatuGetDayUseCase na obsługę IObservable<DayData>
- [ ] 8.2 Refactor FitatuExportMonthCsvUseCase na streaming (NIE POTRZEBNE)
- [ ] 8.3 Refactor SuuntoGetDailyActivitySummaryUseCase na streaming (NIE POTRZEBNE - działa z Observable)
- [ ] 8.4 Refactor SuuntoGetDailySleepSummaryUseCase na streaming (NIE POTRZEBNE - działa z Observable)
- [ ] 8.5 Dodać ToList() dla use case'ów wymagających pełnej listy (NIE POTRZEBNE)
- [ ] 8.6 Dodać FirstAsync() dla use case'ów wymagających pierwszego elementu (NIE POTRZEBNE)

## 9. Cancellation Support

- [x] 9.1 CancellationToken propagowany przez Observable.FromAsync
- [ ] 9.2 Dodać TakeUntil(CancellationToken) operator (NIE POTRZEBNE)
- [ ] 9.3 Zaimplementować endpointy API dla cancellation (POST /api/cancel/{operationId}) (NIE POTRZEBNE)
- [ ] 9.4 Dodać OperationRegistry dla śledzenia aktywnych operacji (NIE POTRZEBNE)
- [ ] 9.5 Zaimplementować graceful cancellation (finish current item, stop next) (NIE POTRZEBNE)

## 10. Progress Reporting

- [ ] 10.1 Utworzyć record StreamingProgress (NIE POTRZEBNE - uproszczone)
- [ ] 10.2 Zaimplementować IProgress<StreamingProgress> (NIE POTRZEBNE)
- [ ] 10.3 Dodać progress events do observable stream (NIE POTRZEBNE)
- [x] 10.4 Podstawowe metody w SignalR hub (opcjonalnie)
- [ ] 10.5 Dodać progress throttling (NIE POTRZEBNE)

## 11. Memory Optimization

- [ ] 11.1 Zaimplementować ArrayPool<byte> (NIE POTRZEBNE - uproszczone)
- [x] 11.2 Używać `using` dla streams (już zaimplementowane)
- [ ] 11.3 Zaimplementować object pooling (NIE POTRZEBNE)
- [ ] 11.4 Dodać GC pressure monitoring (NIE POTRZEBNE)
- [ ] 11.5 Zoptymalizować buffer sizes (NIE POTRZEBNE)

## 19. Feature Flag and Migration

- [x] 19.1 Dodać UseStreamingApi do appsettings.json (default: false)
- [ ] 19.2 Zaimplementować factory pattern (NIE POTRZEBNE - uproszczone)
- [ ] 19.3 Dodać adapter ObservableToTask (NIE POTRZEBNE - uproszczone)
- [ ] 19.4 Przetestować przełączanie flagi
- [ ] 19.5 Przygotować migration plan (legacy -> streaming)

## 20. Cleanup and Optimization

- [ ] 20.1 Usunąć stare implementacje po stabilizacji (2 tygodnie na prod)
- [ ] 20.2 Usunąć feature flag UseStreamingApi
- [ ] 20.3 Zoptymalizować buffer sizes na podstawie production metrics
- [ ] 20.4 Dodać production monitoring (memory, throughput, errors)
- [ ] 20.5 Zarchiwizować legacy code documentation
