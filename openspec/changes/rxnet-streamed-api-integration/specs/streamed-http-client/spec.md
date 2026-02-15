## ADDED Requirements

### Requirement: Streaming HTTP Response
System SHALL support streaming HTTP responses without loading entire body into memory.

#### Scenario: Large API response
- **WHEN** API returns large response (> 1MB or > 100 items)
- **THEN** HTTP client starts reading body as stream immediately after headers
- **AND** data is processed in chunks without loading entire response into memory
- **AND** memory usage remains constant regardless of response size

#### Scenario: Small API response
- **WHEN** API returns small response (< 100KB)
- **THEN** streaming is still used for consistency
- **AND** minimal overhead is acceptable
- **AND** memory usage is still bounded

#### Scenario: HTTP headers read
- **WHEN** HTTP request is sent
- **THEN** client waits only for headers (ResponseHeadersRead option)
- **AND** does not wait for full body
- **AND** starts processing as soon as headers are received

### Requirement: Observable HTTP Client
System SHALL expose HTTP operations as IObservable<T> instead of Task<T>.

#### Scenario: Fetch data as observable
- **WHEN** client calls GetStreamObservable(url)
- **THEN** it receives IObservable<HttpResponseMessage>
- **AND** can compose with RX operators (retry, timeout, etc.)
- **AND** can subscribe to receive data progressively

#### Scenario: Error in observable stream
- **WHEN** HTTP request fails
- **THEN** error is propagated through OnError
- **AND** subscriber can handle error via Subscribe(onError)
- **AND** stream is properly disposed

#### Scenario: Observable completion
- **WHEN** HTTP response is fully read and processed
- **THEN** observable completes (OnCompleted)
- **AND** all resources are released
- **AND** subscriber knows processing is done

### Requirement: Content Stream Reading
System SHALL read HTTP content as Stream for processing.

#### Scenario: Read JSON stream
- **WHEN** response content is available
- **THEN** content is read as Stream (not string/byte[])
- **AND** stream is passed to JSON deserializer
- **AND** stream is disposed after reading

#### Scenario: Stream error handling
- **WHEN** error occurs while reading stream (network error)
- **THEN** stream is disposed properly
- **THEN** error is propagated to observable
- **AND** no resource leak occurs
