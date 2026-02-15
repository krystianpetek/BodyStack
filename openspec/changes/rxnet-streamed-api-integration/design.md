## Context

### Current Architecture
Integracje z Fitatu i Suunto API używają standardowego `HttpClient` z metodami zwracającymi `Task<T>`. Dla dużych zakresów dat (np. rok), aplikacja:
1. Wysyła request HTTP
2. Czeka na pełny response
3. Deserializuje cały JSON do pamięci
4. Zwraca kompletny obiekt

### Problem
- **Memory pressure**: Duże response'y (rok danych = ~MB JSON) trzymane w pamięci
- **Latency**: Użytkownik widzi "loading" przez cały czas pobierania
- **No cancellation**: Nie można anulować w trakcie pobierania
- **No partial results**: Wszystko albo nic

### Constraints
- RX.NET już jest w projekcie
- API zewnętrzne wspierają streaming (HTTP/1.1 chunked lub HTTP/2)
- Minimal API pattern pozostaje
- Nie można zmienić kontraktów publicznych API (frontend niezależny)

### Stakeholders
- Users (szybsze time-to-first-result, możliwość anulowania)
- Developers (łatwiejsze cancellation, backpressure handling)
- System (mniejsze zużycie RAM, lepsza skalowalność)

## Goals / Non-Goals

**Goals:**
1. Strumieniowe pobieranie danych z Fitatu API (Observable<HttpResponseMessage>)
2. Strumieniowe parsowanie JSON (JsonSerializer.DeserializeAsyncEnumerable + RX)
3. Backpressure handling dla dużych datasetów
4. Retry policies z exponential backoff dla HTTP requests
5. Circuit breaker dla ochrony API przed przeciążeniem
6. Cancellation support (użytkownik może anulować)
7. Progress reporting dla długich operacji

**Non-Goals:**
1. Zmiana kontraktów publicznych HTTP API (tylko internal changes)
2. WebSocket connections (pozostaje HTTP)
3. Server-Sent Events (SSE) do frontendu (opcjonalnie w przyszłości)
4. GraphQL (REST pozostaje)
5. Batching/pagination (pozostaje jak jest)

## Decisions

### Decision 1: Observable<HttpResponseMessage> with HttpCompletionOption.ResponseHeadersRead
**Rationale**: ResponseHeadersRead pozwala na streaming - zaczynamy czytać body zanim całe zostanie pobrane.

**Implementation**:
```csharp
var response = await _httpClient.SendAsync(
    request, 
    HttpCompletionOption.ResponseHeadersRead, 
    cancellationToken);
    
var stream = await response.Content.ReadAsStreamAsync();
return Observable.FromAsync(ct => JsonSerializer.DeserializeAsyncEnumerable<T>(stream, _options, ct))
    .SelectMany(x => x.ToObservable());
```

**Alternative Considered**:
- IAsyncEnumerable<T> (dobry, ale RX daje więcej operators)
- Task<IReadOnlyList<T>> (obecne rozwiązanie, brak streaming)

**Trade-off**: RX daje composability (retry, throttle, etc.) kosztem złożoności.

### Decision 2: JsonSerializer.DeserializeAsyncEnumerable
**Rationale**: Najwydajniejszy sposób parsowania dużych JSON arrays bez ładowania całości do pamięci.

**Implementation**:
```csharp
await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<T>(stream, _options, ct))
{
    yield return item;
}
```

**Alternative Considered**:
- Newtonsoft.Json (wolniejszy, więcej pamięci)
- Manual JSON parsing (zbyt niskopoziomowy)
- JArray.Load (ładuje cały JSON do pamięci)

**Trade-off**: System.Text.Json jest fastest i most memory-efficient.

### Decision 3: Backpressure with Buffer and Sample
**Rationale**: Jeśli API zwraca dane szybciej niż możemy przetworzyć, musimy coś zrobić z overflow.

**Implementation**:
```csharp
stream
    .Buffer(TimeSpan.FromMilliseconds(100), 100) // Batchowanie
    .SelectMany(batch => ProcessBatch(batch))
    .Sample(TimeSpan.FromSeconds(1)) // Throttling dla UI updates
    .Subscribe();
```

**Alternative Considered**:
- OnBackpressureBuffer (nie ma wbudowanego w RX.NET, trzeba implementować)
- OnBackpressureDrop (gubimy dane, ale system stabilny)
- OnBackpressureLatest (tylko najnowsze dane)

**Trade-off**: Buffer + Sample to kompromis między throughput a memory.

### Decision 4: Polly for Retry and Circuit Breaker
**Rationale**: Polly to industry standard dla resilience patterns w .NET. Bardziej feature-rich niż RX retry operators.

**Implementation**:
```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var circuitBreaker = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1));

var combined = Policy.WrapAsync(circuitBreaker, retryPolicy);

return await combined.ExecuteAsync(() => _httpClient.GetStreamAsync(url));
```

**Alternative Considered**:
- RX Retry operator (prostszy, ale mniej features)
- Własna implementacja (reinventing the wheel)

**Trade-off**: Dodajemy dependency (Polly), ale zyskujemy battle-tested solution.

### Decision 5: Cancellation via TakeUntil and CancellationToken
**Rationale**: Użytkownik musi móc anulować długie operacje (np. zmiana zdania po kliknięciu "Fetch Year").

**Implementation**:
```csharp
public IObservable<T> FetchData(CancellationToken externalToken)
{
    var cancellationObservable = Observable
        .FromEventPattern(
            h => externalToken.Register(h),
            h => { /* unregister */ })
        .Select(_ => Unit.Default);
    
    return FetchDataStream()
        .TakeUntil(cancellationObservable)
        .Finally(() => _logger.LogInformation("Fetch cancelled"));
}
```

**Alternative Considered**:
- IAsyncEnumerable z CancellationToken (prostsze, ale mniej control)
- CancellationTokenSource manualne (więcej kodu)

**Trade-off**: RX cancellation jest deklaratywne i composable.

### Decision 6: Observable<T> Return Type for Use Cases
**Rationale**: Use case'y zwracają Observable zamiast Task<IReadOnlyList<T>>. Frontend może subskrybować i otrzymywać dane na bieżąco.

**Implementation**:
```csharp
// Stare API
public async Task<IReadOnlyList<DayData>> GetMonthData(string yearMonth);

// Nowe API
public IObservable<DayData> GetMonthData(string yearMonth);
```

**Alternative Considered**:
- IAsyncEnumerable<T> (dobre, ale wymaga await foreach)
- Task<IReadOnlyList<T>> z IProgress<T> (bardziej skomplikowane)

**Trade-off**: Breaking change w internal API, ale frontend niezależny (HTTP API bez zmian).

### Decision 7: Separate Streams for Different Data Types
**Rationale**: Fitatu dane żywieniowe vs Suunto dane aktywności/snów to różne domeny. Osobne strumienie zapobiegają couplingowi.

**Implementation**:
```csharp
// Fitatu
IObservable<FitatuDayPlan> GetDayPlans(DateRange range);

// Suunto
IObservable<SuuntoActivity> GetActivities(DateRange range);
IObservable<SuuntoSleep> GetSleepData(DateRange range);
```

**Alternative Considered**:
- Jeden uniwersalny stream (zbyt ogólny, traci type safety)

**Trade-off**: Więcej kodu, ale lepsza separacja i testowalność.

## Risks / Trade-offs

### Risk 1: Debugging Streaming Code
**Risk**: Strumieniowe kod jest trudniejszy do debugowania (async, observables, operators).
**Mitigation**:
- Dobre logowanie na każdym etapie (Do operator)
- Testy jednostkowe dla każdego operatora
- Wizualizacja flow danych w dokumentacji

### Risk 2: Backwards Compatibility
**Risk**: Zmiana return type z Task<T> na IObservable<T> to breaking change dla internal API.
**Mitigation**:
- Feature flag dla nowej implementacji
- Adapter pattern (można wrapper Task<T> na Observable)
- Gradual migration per use case

### Risk 3: Memory Leaks from Unobserved Streams
**Risk**: Jeśli strumień nie jest prawidłowo dispose'owany, może wyciekać pamięć.
**Mitigation**:
- Użyj using/Dispose pattern zawsze
- Timeout dla niekończących się strumieni
- Monitoring subskrypcji

### Risk 4: Performance Overhead
**Risk**: RX + streaming może być wolniejszy niż prosty Task<T> dla małych response'ów.
**Mitigation**:
- Threshold: używaj streaming tylko dla >100 items lub >1MB response
- Benchmark: porównaj performance przed i po
- Opt-in/opt-out per endpoint

### Risk 5: External API Limitations
**Risk**: Nie wszystkie API wspierają streaming (HTTP/1.1 bez chunked encoding).
**Mitigation**:
- Fallback na standardowy Task<T> jeśli streaming nie jest wspierany
- Sprawdź API documentation przed implementacją

## Migration Plan

### Phase 1: Infrastructure
1. Dodaj Polly do projektu (NuGet)
2. Utwórz ReactiveHttpClient wrapper
3. Zaimplementuj extension methods dla streaming JSON

### Phase 2: Fitatu Integration
1. Refactor FitatuClient na Observable
2. Dodaj retry i circuit breaker
3. Przepisz use case'y
4. Testuj w dev

### Phase 3: Suunto Integration
1. Refactor Suunto clients na Observable
2. Analogicznie jak Fitatu
3. Testuj w dev

### Phase 4: Performance Testing
1. Porównaj memory usage (streaming vs buffering)
2. Porównaj latency (time-to-first-result)
3. Zmierz throughput

### Phase 5: Cleanup
1. Usuń stare implementacje
2. Usuń feature flags
3. Aktualizuj dokumentację

### Rollback Strategy
- Feature flag pozwala na powrót do starej implementacji
- Adapter pattern pozwala na wrapper Observable -> Task jeśli potrzeba
- HTTP API bez zmian - frontend nie wie o zmianie

## Open Questions

1. **Q**: Czy Fitatu API wspiera streaming (HTTP chunked encoding)?
   **A**: Trzeba sprawdzić w dokumentacji API. Jeśli nie, fallback na standardowy HttpClient.

2. **Q**: Czy potrzebujemy buffering dla małych response'ów (<100 items)?
   **A**: Zacznijmy od streaming dla wszystkiego, optymalizujemy jeśli potrzeba.

3. **Q**: Jak obsługiwać rate limiting (429 Too Many Requests) z Polly?
   **A**: Polly ma retry z exponential backoff. Dla 429 można dodać custom policy.

4. **Q**: Czy cancellation powinno być per-request czy globalne?
   **A**: Per-request (CancellationToken per API call) daje więcej control.

5. **Q**: Jakie są limity pamięci dla streaming (max buffer size)?
   **A**: Domyślnie 100 items lub 1MB. Konfigurowalne.
