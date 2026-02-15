using System.Globalization;
using System.IO.Compression;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BodyStack.Server.Integrations.Suunto;
using Microsoft.Extensions.Caching.Memory;

namespace BodyStack.Server.Application.Suunto;

public sealed class SuuntoGetDailySleepSummaryUseCase
{
    private readonly ISuuntoSleepExportClient _client;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SuuntoGetDailySleepSummaryUseCase> _logger;

    public SuuntoGetDailySleepSummaryUseCase(
        ISuuntoSleepExportClient client,
        IMemoryCache cache,
        ILogger<SuuntoGetDailySleepSummaryUseCase> logger)
    {
        _client = client;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SuuntoDailySleepSummary>> ExecuteAsync(
        string sttAuthorization,
        TimeSpan ttl,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(sttAuthorization);
        var cacheKey = $"suunto.sleep.daily.{tokenHash}.{from?.ToString("yyyy-MM-dd") ?? "_"}.{to?.ToString("yyyy-MM-dd") ?? "_"}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<SuuntoDailySleepSummary>? cached) && cached is not null)
        {
            return cached;
        }

        await EnsureStagesCachedAsync(sttAuthorization, tokenHash, ttl, cancellationToken);

        var ndjsonStream = await GetNdjsonStreamAsync(
            sttAuthorization,
            tokenHash,
            ttl,
            cacheFilePrefix: "sleep",
            fetch: (token, ct) => _client.GetSleepExportAsync(token, ct)
            .FirstAsync()
            .ToTask(ct), cancellationToken);

        await using (ndjsonStream)
        {
            var summaries = await AggregateAsync(ndjsonStream, from, to, cancellationToken);
            _cache.Set(cacheKey, summaries, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });
            return summaries;
        }
    }

    private async Task EnsureStagesCachedAsync(string sttAuthorization, string tokenHash, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var cacheKey = $"suunto.sleepstages.cached.{tokenHash}";
        if (_cache.TryGetValue(cacheKey, out bool already) && already)
        {
            return;
        }

        await using var _ = await GetNdjsonStreamAsync(
            sttAuthorization,
            tokenHash,
            ttl,
            cacheFilePrefix: "sleepstages",
            fetch: (token, ct) => _client.GetSleepStagesExportAsync(token, ct)
            .FirstAsync()
            .ToTask(ct), cancellationToken);

        _cache.Set(cacheKey, true, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
    }

    private async Task<Stream> GetNdjsonStreamAsync(
        string sttAuthorization,
        string tokenHash,
        TimeSpan ttl,
        string cacheFilePrefix,
        Func<string, CancellationToken, Task<HttpResponseMessage>> fetch,
        CancellationToken cancellationToken)
    {
        var cacheDir = Path.Combine(AppContext.BaseDirectory, "cache", "suunto");
        Directory.CreateDirectory(cacheDir);

        var dataPath = Path.Combine(cacheDir, $"{cacheFilePrefix}-{tokenHash}.ndjson");
        var metaPath = Path.Combine(cacheDir, $"{cacheFilePrefix}-{tokenHash}.meta");

        if (TryUseFileCache(metaPath, dataPath, ttl))
        {
            return File.OpenRead(dataPath);
        }

        using var resp = await fetch(sttAuthorization, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await SafeReadBodyAsync(resp, cancellationToken);
            throw new InvalidOperationException($"Suunto {cacheFilePrefix} export failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}");
        }

        await using var networkStream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        await using var decoded = await TryDecodeContentAsync(resp, networkStream, cancellationToken);

        await using (var fs = File.Create(dataPath))
        {
            await decoded.CopyToAsync(fs, cancellationToken);
        }

        await File.WriteAllTextAsync(metaPath, DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture), cancellationToken);

        return File.OpenRead(dataPath);
    }

    private async Task<IReadOnlyList<SuuntoDailySleepSummary>> AggregateAsync(
        Stream ndjsonStream,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        using var sr = new StreamReader(ndjsonStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 64 * 1024, leaveOpen: true);

        var bestBySession = new Dictionary<(long SleepId, bool IsNap), SuuntoSleepEntry>();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await sr.ReadLineAsync(cancellationToken);
            if (line is null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;

            SuuntoSleepEntry? entry;
            try
            {
                entry = ParseEntry(line);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse Suunto sleep NDJSON line. Skipping.");
                continue;
            }

            var key = (entry.EntryData.SleepId, entry.EntryData.IsNap);
            if (!bestBySession.TryGetValue(key, out var existing))
            {
                bestBySession[key] = entry;
                continue;
            }

            // sleep/export appears to contain multiple snapshots for the same sleep session.
            // Pick the most complete record (max duration), then prefer the latest timestamp.
            var takeNew = entry.EntryData.DurationSeconds > existing.EntryData.DurationSeconds ||
                          (Math.Abs(entry.EntryData.DurationSeconds - existing.EntryData.DurationSeconds) < 0.0001 && entry.Timestamp > existing.Timestamp);

            if (takeNew)
            {
                bestBySession[key] = entry;
            }
        }

        var map = new Dictionary<DateOnly, Accumulator>();

        foreach (var entry in bestBySession.Values)
        {
            var start = entry.Timestamp;
            var end = start.AddSeconds(entry.EntryData.DurationSeconds);

            var day = entry.EntryData.IsNap
                ? DateOnly.FromDateTime(start.LocalDateTime.Date)
                : DateOnly.FromDateTime(end.LocalDateTime.Date);

            if (from.HasValue && day < from.Value) continue;
            if (to.HasValue && day > to.Value) continue;

            if (!map.TryGetValue(day, out var acc))
            {
                acc = new Accumulator();
                map[day] = acc;
            }

            if (entry.EntryData.IsNap)
            {
                acc.NapSeconds += entry.EntryData.DurationSeconds;
                acc.NapCount++;
            }
            else
            {
                acc.NightSeconds += entry.EntryData.DurationSeconds;
                acc.NightCount++;
            }
        }

        return map
            .OrderBy(kvp => kvp.Key)
            .Select(kvp =>
            {
                var (day, acc) = (kvp.Key, kvp.Value);
                var total = acc.NightSeconds + acc.NapSeconds;
                return new SuuntoDailySleepSummary(
                    Date: day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    TotalSleepSeconds: total,
                    NightSleepSeconds: acc.NightSeconds,
                    NapSleepSeconds: acc.NapSeconds,
                    SleepSessionsCount: acc.NightCount,
                    NapSessionsCount: acc.NapCount);
            })
            .ToArray();
    }

    private static SuuntoSleepEntry ParseEntry(string line)
    {
        using var doc = JsonDocument.Parse(line);
        var root = doc.RootElement;

        var timestamp = root.GetProperty("timestamp").GetString();
        if (timestamp is null)
        {
            throw new FormatException("Missing timestamp");
        }

        var ts = DateTimeOffset.Parse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        var dataElement = root.GetProperty("entryData");
        var data = SuuntoSleepEntryData.FromJsonElement(dataElement);

        return new SuuntoSleepEntry(ts, data);
    }

    private static bool TryUseFileCache(string metaPath, string dataPath, TimeSpan ttl)
    {
        if (!File.Exists(metaPath) || !File.Exists(dataPath)) return false;

        var meta = File.ReadAllText(metaPath).Trim();
        if (!DateTimeOffset.TryParse(meta, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var fetchedAt))
        {
            return false;
        }

        return DateTimeOffset.UtcNow - fetchedAt < ttl;
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static async Task<Stream> TryDecodeContentAsync(HttpResponseMessage resp, Stream networkStream, CancellationToken cancellationToken)
    {
        if (resp.Content.Headers.ContentEncoding.Any(e => string.Equals(e, "gzip", StringComparison.OrdinalIgnoreCase)))
        {
            return new GZipStream(networkStream, CompressionMode.Decompress, leaveOpen: false);
        }

        var buffered = new MemoryStream();
        await networkStream.CopyToAsync(buffered, cancellationToken);
        buffered.Position = 0;

        if (buffered.Length >= 2)
        {
            var b1 = buffered.ReadByte();
            var b2 = buffered.ReadByte();
            buffered.Position = 0;
            if (b1 == 0x1f && b2 == 0x8b)
            {
                return new GZipStream(buffered, CompressionMode.Decompress, leaveOpen: false);
            }
        }

        return buffered;
    }

    private static async Task<string> SafeReadBodyAsync(HttpResponseMessage resp, CancellationToken cancellationToken)
    {
        try
        {
            var s = await resp.Content.ReadAsStringAsync(cancellationToken);
            return s.Length > 500 ? s[..500] : s;
        }
        catch
        {
            return string.Empty;
        }
    }

    private sealed class Accumulator
    {
        public double NightSeconds;
        public double NapSeconds;
        public int NightCount;
        public int NapCount;
    }
}
