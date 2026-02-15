## ADDED Requirements

### Requirement: JSON Streaming Deserialization
System SHALL deserialize JSON arrays as stream without loading entire array into memory.

#### Scenario: Deserialize large JSON array
- **WHEN** API returns JSON array with 1000+ items
- **THEN** each item is deserialized individually as stream is read
- **AND** items are yielded one by one via IAsyncEnumerable
- **AND** memory usage is O(1) per item, not O(n) for entire array

#### Scenario: Observable from JSON stream
- **WHEN** IAsyncEnumerable<T> is produced from JSON stream
- **THEN** it is converted to IObservable<T>
- **AND** each JSON object becomes observable event
- **AND** subscriber receives items as they are deserialized

#### Scenario: JSON parsing error
- **WHEN** JSON is malformed during streaming
- **THEN** JsonException is thrown
- **AND** stream is disposed
- **AND** error is propagated through observable
- **AND** partial results may be lost (acceptable trade-off)

### Requirement: System.Text.Json Usage
System SHALL use System.Text.Json for streaming deserialization.

#### Scenario: Utf8JsonReader streaming
- **WHEN** JSON stream is being read
- **THEN** Utf8JsonReader is used for high-performance parsing
- **AND** JsonSerializerOptions are respected
- **AND** performance is optimal

#### Scenario: DeserializeAsyncEnumerable
- **WHEN** deserializing JSON array stream
- **THEN** JsonSerializer.DeserializeAsyncEnumerable<T> is used
- **AND** items are yielded asynchronously
- **AND** cancellation token is respected

#### Scenario: Custom converter support
- **WHEN** JSON contains custom types
- **THEN** custom JsonConverters are used during streaming
- **AND** converters work correctly with streaming
- **AND** no buffering is required

### Requirement: Type-Safe Streaming
System SHALL maintain type safety during JSON streaming.

#### Scenario: Strongly-typed observable
- **WHEN** streaming Fitatu data
- **THEN** observable is IObservable<FitatuDayPlan>
- **AND** type safety is preserved
- **AND** no dynamic/loose typing

#### Scenario: Generic streaming method
- **WHEN** creating generic streaming client
- **THEN** method is generic <T> where T : class
- **AND** works with any JSON-serializable type
- **AND** maintains compile-time type safety
