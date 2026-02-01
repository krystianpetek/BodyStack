using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace BodyStack.Server.Integrations.Fitatu;

public sealed class FitatuClient : IFitatuClient
{
    private readonly HttpClient _httpClient;
    private readonly FitatuOptions _options;

    public FitatuClient(HttpClient httpClient, IOptions<FitatuOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<FitatuLoginResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/login")
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("_username", username),
                new KeyValuePair<string, string>("_password", password),
            ])
        };

        request.Headers.TryAddWithoutValidation("api-key", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("api-secret", _options.ApiSecret ?? string.Empty);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

        var root = json.RootElement;

        var token = root.TryGetProperty("token", out var tokenElement) ? tokenElement.GetString() : null;
        var refreshToken = root.TryGetProperty("refresh_token", out var refreshTokenElement) ? refreshTokenElement.GetString() : null;

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new InvalidOperationException("Fitatu login response did not contain token and refresh_token.");
        }

        return new FitatuLoginResult(token, refreshToken);
    }

    public async Task<JsonDocument> GetDietAndActivityPlanDayAsync(
        string userId,
        DateOnly date,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token is required.", nameof(token));
        }

        var url = $"/api/diet-and-activity-plan/{Uri.EscapeDataString(userId)}/day/{date:yyyy-MM-dd}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("api-key", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("api-secret", _options.ApiSecret ?? string.Empty);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
    }
}
