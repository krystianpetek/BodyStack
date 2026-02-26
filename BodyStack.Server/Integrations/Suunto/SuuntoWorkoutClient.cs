using System.Net.Http.Headers;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;

namespace BodyStack.Server.Integrations.Suunto;

public sealed class SuuntoWorkoutClient : ISuuntoWorkoutClient
{
    private readonly HttpClient _httpClient;

    public SuuntoWorkoutClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.sports-tracker.com", UriKind.Absolute);
    }

    public IObservable<JsonDocument> GetWorkoutsAsync(
        string sttAuthorization, 
        CancellationToken cancellationToken = default)
    {
        return Observable.FromAsync(async ct =>
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, 
                "/apiserver/v1/workouts?limited=true&limit=1000000");
            
            request.Headers.TryAddWithoutValidation("sttauthorization", sttAuthorization);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonDocument.Parse(content);
        });
    }
}
