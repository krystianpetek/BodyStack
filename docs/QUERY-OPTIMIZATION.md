# Query Optimization Patterns

This document describes the query optimization patterns used in BodyStack to ensure efficient database access.

## Anti-Pattern: Loading All Data Into Memory

### Problem

The following pattern was found in the codebase:

```csharp
// ❌ BAD: Loads ALL records into memory, then filters
var entities = await _db.FitatuSessions
    .AsNoTracking()
    .ToListAsync(cancellationToken);  // ← Loads entire table!

var entity = entities
    .OrderByDescending(x => x.UpdatedAt)
    .FirstOrDefault();
```

**Problems:**
- **Memory**: Loads entire table into application memory (O(n) space)
- **Performance**: Database sends all data over the wire
- **Scalability**: Gets worse as table grows
- **Latency**: Multiple round-trips (fetch all, then process)

## Solution: Database-Side Processing

### Pattern

Move the ordering and filtering to the database:

```csharp
// ✅ GOOD: Database handles sorting and limiting
var entity = await _db.FitatuSessions
    .AsNoTracking()
    .OrderByDescending(x => x.UpdatedAt)
    .FirstOrDefaultAsync(cancellationToken);  // ← Single optimized query
```

**Benefits:**
- **Memory**: Only one record loaded (O(1) space)
- **Performance**: Database uses indexes efficiently
- **Scalability**: Constant memory regardless of table size
- **Latency**: Single round-trip

## Examples

### Getting Latest Record

**Before:**
```csharp
var entities = await _db.Sessions.ToListAsync();
var latest = entities.OrderByDescending(x => x.UpdatedAt).FirstOrDefault();
```

**After:**
```csharp
var latest = await _db.Sessions
    .OrderByDescending(x => x.UpdatedAt)
    .FirstOrDefaultAsync();
```

### Filtering with Pagination

**Before:**
```csharp
var all = await _db.Items.ToListAsync();
var filtered = all.Where(x => x.Status == "Active").ToList();
var page = filtered.Skip(100).Take(20).ToList();
```

**After:**
```csharp
var page = await _db.Items
    .Where(x => x.Status == "Active")
    .Skip(100)
    .Take(20)
    .ToListAsync();
```

### Counting with Filter

**Before:**
```csharp
var all = await _db.Orders.ToListAsync();
var count = all.Count(x => x.Status == "Pending");
```

**After:**
```csharp
var count = await _db.Orders
    .CountAsync(x => x.Status == "Pending");
```

## When to Use Each Approach

### Always Use Database-Side Processing For:

1. **Filtering** (`Where`) - Never filter in memory after loading
2. **Sorting** (`OrderBy`) - Let the database use indexes
3. **Pagination** (`Skip`/`Take`) - Essential for large datasets
4. **Aggregation** (`Count`, `Sum`, `Average`) - Much faster in database
5. **Top N Queries** (`First`, `Take`) - Use `FirstOrDefaultAsync`, not `ToListAsync().First()`

### It's OK to Load Into Memory When:

1. **Small reference data** (e.g., status codes, configuration)
2. **Complex in-memory calculations** that can't be expressed in SQL
3. **Client-side caching** scenarios
4. **Data export** where you actually need everything

## Checking for Issues

To find potential issues in your code:

1. Look for `ToListAsync()` followed by LINQ operations
2. Check if you're loading data you don't immediately use
3. Monitor memory usage in production
4. Use database query logs to identify slow queries

## Performance Monitoring

Consider adding query timing logs:

```csharp
var stopwatch = Stopwatch.StartNew();
var result = await _db.Sessions
    .OrderByDescending(x => x.UpdatedAt)
    .FirstOrDefaultAsync();
stopwatch.Stop();

if (stopwatch.ElapsedMilliseconds > 100)
{
    _logger.LogWarning("Slow query detected: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
}
```

## Database Indexes

For optimal performance, ensure appropriate indexes exist:

```sql
-- For UpdatedAt sorting
CREATE INDEX idx_fitat sessions_updated_at ON "FitatuSessions"("UpdatedAt" DESC);

-- For Status filtering
CREATE INDEX idx_items_status ON "Items"("Status") WHERE "Status" = 'Active';
```

## Summary

| Scenario | Anti-Pattern | Pattern |
|----------|-------------|---------|
| Get latest | `.ToListAsync().OrderBy().First()` | `.OrderBy().FirstOrDefaultAsync()` |
| Filter | `.ToListAsync().Where()` | `.Where().ToListAsync()` |
| Count | `.ToListAsync().Count()` | `.CountAsync()` |
| Pagination | `.ToListAsync().Skip().Take()` | `.Skip().Take().ToListAsync()` |

**Golden Rule**: Push as much work as possible to the database. Only load data you actually need.
