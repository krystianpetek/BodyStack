## ADDED Requirements

### Requirement: Retry with Exponential Backoff
System SHALL retry failed HTTP requests with exponential backoff.

#### Scenario: Transient network error
- **WHEN** HTTP request fails with HttpRequestException (network error)
- **THEN** request is retried after 1 second
- **AND** second retry after 2 seconds
- **AND** third retry after 4 seconds
- **AND** maximum 3 retries (configurable)

#### Scenario: Non-retryable error
- **WHEN** HTTP request fails with 4xx error (client error)
- **THEN** no retry is attempted
- **AND** error is returned immediately
- **AND** exception is propagated

#### Scenario: Success after retry
- **WHEN** request fails first time but succeeds on retry
- **THEN** successful response is returned
- **AND** retry count is logged as warning
- **AND** no error is thrown

### Requirement: Circuit Breaker Pattern
System SHALL implement circuit breaker to prevent cascading failures.

#### Scenario: Circuit breaker closed
- **WHEN** failure rate is below threshold
- **THEN** circuit remains closed
- **AND** requests are processed normally
- **AND** failures are tracked

#### Scenario: Circuit breaker opens
- **WHEN** 5 consecutive failures occur within 1 minute
- **THEN** circuit opens
- **AND** new requests fail immediately with CircuitBreakerOpenException
- **AND** no actual HTTP calls are made

#### Scenario: Circuit breaker half-open
- **WHEN** circuit has been open for 1 minute
- **THEN** circuit transitions to half-open
- **AND** next request is allowed through as probe
- **AND** if success: circuit closes, if failure: circuit opens

### Requirement: Rate Limiting
System SHALL respect rate limits of external APIs.

#### Scenario: Rate limit detection
- **WHEN** API returns 429 Too Many Requests
- **THEN** rate limit is detected
- **AND** client backs off according to Retry-After header
- **AND** request is retried after specified delay

#### Scenario: Proactive rate limiting
- **WHEN** making requests to rate-limited API
- **THEN** client limits requests to X per second
- **AND** no 429 errors are received (ideally)
- **AND** throughput is controlled

#### Scenario: Rate limit token bucket
- **WHEN** using TokenBucket rate limiter
- **THEN** tokens are consumed per request
- **AND** tokens regenerate over time
- **AND** requests wait if no tokens available

### Requirement: Timeout Handling
System SHALL handle timeouts for HTTP requests.

#### Scenario: Request timeout
- **WHEN** HTTP request takes longer than timeout (e.g., 30s)
- **THEN** OperationCanceledException is thrown
- **AND** request is cancelled
- **AND** error is propagated

#### Scenario: Per-operation timeout
- **WHEN** different operations have different timeout requirements
- **THEN** timeout is configurable per operation
- **AND** default timeout is used if not specified
- **AND** TimeoutException is thrown on timeout

#### Scenario: Overall operation timeout
- **WHEN** entire streaming operation has timeout
- **THEN** Timeout operator in RX is used
- **AND** stream is cancelled after timeout
- **AND** partial results may be returned
