using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using BodyStack.Server.Domain.Services;
using BodyStack.Server.Integrations.Suunto;
using Microsoft.Extensions.Caching.Memory;

namespace BodyStack.Server.Application.Suunto;

public sealed record SuuntoUserProfile(
    double WeightKg,
    double HeightCm,
    int Age,
    string Gender,
    string? Name,
    string? Email);

public sealed class SuuntoGetUserProfileUseCase
{
    private readonly ISuuntoUserClient _client;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SuuntoGetUserProfileUseCase> _logger;

    public SuuntoGetUserProfileUseCase(
        ISuuntoUserClient client,
        IMemoryCache cache,
        ILogger<SuuntoGetUserProfileUseCase> logger)
    {
        _client = client;
        _cache = cache;
        _logger = logger;
    }

    public async Task<SuuntoUserProfile> ExecuteAsync(
        string sttAuthorization,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"suunto.user.profile.{HashToken(sttAuthorization)}";
        
        if (_cache.TryGetValue(cacheKey, out SuuntoUserProfile? cached) && cached is not null)
        {
            return cached;
        }

        var json = await _client.GetUserSettingsAsync(sttAuthorization, cancellationToken)
            .FirstAsync()
            .ToTask(cancellationToken);

        var payload = json.RootElement.GetProperty("payload");
        
        // Parse weight (stored in grams, convert to kg)
        var weightG = payload.GetProperty("weight").GetDouble();
        var weightKg = weightG / 1000.0;
        
        // Parse height (stored in cm)
        var heightCm = payload.GetProperty("height").GetDouble();
        
        // Parse gender
        var gender = payload.GetProperty("gender").GetString() ?? "MALE";
        
        // Calculate age from birthdate
        var birthdateMs = payload.GetProperty("birthdate").GetInt64();
        var birthdate = DateTimeOffset.FromUnixTimeMilliseconds(birthdateMs);
        var age = CalculateAge(birthdate);
        
        // Optional fields
        var name = payload.TryGetProperty("realName", out var nameEl) ? nameEl.GetString() : null;
        var email = payload.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null;

        var profile = new SuuntoUserProfile(weightKg, heightCm, age, gender, name, email);

        _cache.Set(cacheKey, profile, TimeSpan.FromHours(1));
        return profile;
    }

    private static int CalculateAge(DateTimeOffset birthdate)
    {
        var today = DateTimeOffset.UtcNow;
        var age = today.Year - birthdate.Year;
        if (birthdate.Date > today.AddYears(-age).Date)
        {
            age--;
        }
        return age;
    }

    private static string HashToken(string token)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
