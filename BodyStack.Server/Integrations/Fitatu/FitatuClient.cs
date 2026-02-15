using System.Net.Http.Headers;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using Microsoft.Extensions.Options;
using BodyStack.Server.Infrastructure.Http.Resilience;

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

    public IObservable<FitatuLoginResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromAsync(async ct =>
        {
            var policy = ResiliencePolicyFactory.CreateStreamingPolicy();
            
            return await policy.ExecuteAsync(async innerCt =>
            {
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

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, innerCt);
                response.EnsureSuccessStatusCode();

                await using var responseStream = await response.Content.ReadAsStreamAsync(innerCt);
                using var json = await JsonDocument.ParseAsync(responseStream, cancellationToken: innerCt);

                var root = json.RootElement;
                var authToken = root.TryGetProperty("token", out var tokenElement) ? tokenElement.GetString() : null;
                var refreshToken = root.TryGetProperty("refresh_token", out var refreshTokenElement) ? refreshTokenElement.GetString() : null;

                if (string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(refreshToken))
                {
                    throw new InvalidOperationException("Fitatu login response did not contain token and refresh_token.");
                }

                return new FitatuLoginResult(authToken, refreshToken);
            }, ct);
        });
    }

    public IObservable<JsonDocument> GetDietAndActivityPlanDayAsync(
        string userId,
        DateOnly date,
        string token,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/diet-and-activity-plan/{Uri.EscapeDataString(userId)}/day/{date:yyyy-MM-dd}";

        return Observable.FromAsync(async ct =>
        {
            var policy = ResiliencePolicyFactory.CreateStreamingPolicy();
            
            return await policy.ExecuteAsync(async innerCt =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("api-key", _options.ApiKey);
                request.Headers.TryAddWithoutValidation("api-secret", _options.ApiSecret ?? string.Empty);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, innerCt);
                response.EnsureSuccessStatusCode();

                await using var responseStream = await response.Content.ReadAsStreamAsync(innerCt);
                return await JsonDocument.ParseAsync(responseStream, cancellationToken: innerCt);
            }, ct);
        });
    }
}
