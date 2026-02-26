using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;

namespace BodyStack.Server.Integrations.Suunto;

public interface ISuuntoUserClient
{
    IObservable<JsonDocument> GetUserSettingsAsync(string sttAuthorization, CancellationToken cancellationToken = default);
}

public sealed class SuuntoUserClient : ISuuntoUserClient
{
    private readonly HttpClient _httpClient;

    public SuuntoUserClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.sports-tracker.com", UriKind.Absolute);
    }

    public IObservable<JsonDocument> GetUserSettingsAsync(string sttAuthorization, CancellationToken cancellationToken = default)
    {
        return Observable.FromAsync(async ct =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/apiserver/v1/user/settings");
            request.Headers.TryAddWithoutValidation("sttauthorization", sttAuthorization);

            using var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonDocument.Parse(content);
        });
    }
}
