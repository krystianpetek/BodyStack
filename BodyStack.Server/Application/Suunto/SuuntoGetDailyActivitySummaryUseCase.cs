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

public sealed class SuuntoGetDailyActivitySummaryUseCase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ISuuntoActivityExportClient _client;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SuuntoGetDailyActivitySummaryUseCase> _logger;

    public SuuntoGetDailyActivitySummaryUseCase(
        ISuuntoActivityExportClient client,
        IMemoryCache cache,
        ILogger<SuuntoGetDailyActivitySummaryUseCase> logger)
    {
        _client = client;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SuuntoDailyActivitySummary>> ExecuteAsync(
        string sttAuthorization,
        TimeSpan ttl,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(sttAuthorization);
        var cacheKey = $"suunto.activity.daily.{tokenHash}.{from?.ToString("yyyy-MM-dd") ?? "_"}.{to?.ToString("yyyy-MM-dd") ?? "_"}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<SuuntoDailyActivitySummary>? cached) && cached is not null)
        {
            return cached;
        }

        var linesStream = await GetNdjsonStreamAsync(sttAuthorization, tokenHash, ttl, cancellationToken);
        await using (linesStream)
        {
            var summaries = await AggregateAsync(linesStream, from, to, cancellationToken);
            _cache.Set(cacheKey, summaries, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });
            return summaries;
        }
    }

    private async Task<Stream> GetNdjsonStreamAsync(
        string sttAuthorization,
        string tokenHash,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var cacheDir = Path.Combine(AppContext.BaseDirectory, "cache", "suunto");
        Directory.CreateDirectory(cacheDir);

        var dataPath = Path.Combine(cacheDir, $"activity-{tokenHash}.ndjson");
        var metaPath = Path.Combine(cacheDir, $"activity-{tokenHash}.meta");

        if (TryUseFileCache(metaPath, dataPath, ttl))
        {
            return File.OpenRead(dataPath);
        }

        using var resp = await _client.GetActivityExportAsync(sttAuthorization, cancellationToken)
        .FirstAsync()
        .ToTask(cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await SafeReadBodyAsync(resp, cancellationToken);
            throw new InvalidOperationException($"Suunto export failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. {body}");
        }

        await using var networkStream = await resp.Content.ReadAsStreamAsync(cancellationToken);

        // Some responses may already be plain NDJSON even if request advertises gzip.
        await using var decoded = await TryDecodeContentAsync(resp, networkStream, cancellationToken);

        // Persist to file cache (for TTL + to avoid re-downloading).
        await using (var fs = File.Create(dataPath))
        {
            await decoded.CopyToAsync(fs, cancellationToken);
        }

        await File.WriteAllTextAsync(metaPath, DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture), cancellationToken);

        return File.OpenRead(dataPath);
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

    private async Task<IReadOnlyList<SuuntoDailyActivitySummary>> AggregateAsync(
        Stream ndjsonStream,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        using var sr = new StreamReader(ndjsonStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 64 * 1024, leaveOpen: true);

        var map = new Dictionary<DateOnly, Accumulator>();
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await sr.ReadLineAsync(cancellationToken);
            if (line is null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;

            SuuntoActivityEntry? entry;
            try
            {
                entry = ParseEntry(line);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse Suunto NDJSON line. Skipping.");
                continue;
            }

            var date = DateOnly.FromDateTime(entry.Timestamp.LocalDateTime.Date);
            if (from.HasValue && date < from.Value) continue;
            if (to.HasValue && date > to.Value) continue;

            if (!map.TryGetValue(date, out var acc))
            {
                acc = new Accumulator();
                map[date] = acc;
            }

            acc.Samples++;

            if (entry.EntryData.StepCount is int steps)
            {
                acc.Steps += steps;
            }

            if (entry.EntryData.EnergyConsumption is double energy)
            {
                acc.Energy += energy;
            }

            if (entry.EntryData.Hr is double hr)
            {
                acc.HrSum += hr;
                acc.HrCount++;
            }

            if (entry.EntryData.Hrv is double hrv)
            {
                acc.HrvSum += hrv;
                acc.HrvCount++;
            }
        }

        return map
            .OrderBy(kvp => kvp.Key)
            .Select(kvp =>
            {
                var (day, acc) = (kvp.Key, kvp.Value);
                return new SuuntoDailyActivitySummary(
                    Date: day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Steps: acc.Steps,
                    EnergyConsumption: acc.Energy,
                    AvgHr: acc.HrCount > 0 ? acc.HrSum / acc.HrCount : null,
                    AvgHrv: acc.HrvCount > 0 ? acc.HrvSum / acc.HrvCount : null,
                    Samples: acc.Samples);
            })
            .ToArray();
    }

    private static SuuntoActivityEntry ParseEntry(string line)
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
        var data = SuuntoActivityEntryData.FromJsonElement(dataElement);

        return new SuuntoActivityEntry(ts, data);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static async Task<Stream> TryDecodeContentAsync(HttpResponseMessage resp, Stream networkStream, CancellationToken cancellationToken)
    {
        // If the response is already decompressed by HttpClient, Content-Encoding might be empty.
        // We still need to handle a raw gzip stream if it comes through.
        if (resp.Content.Headers.ContentEncoding.Any(e => string.Equals(e, "gzip", StringComparison.OrdinalIgnoreCase)))
        {
            return new GZipStream(networkStream, CompressionMode.Decompress, leaveOpen: false);
        }

        // Heuristic sniffing: gzip magic header 1F 8B
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
        public int Steps;
        public double Energy;
        public int Samples;
        public double HrSum;
        public int HrCount;
        public double HrvSum;
        public int HrvCount;
    }
}
