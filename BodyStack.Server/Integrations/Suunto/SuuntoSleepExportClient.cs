using System.Net.Http.Headers;
using System.Reactive;
using System.Reactive.Linq;
using BodyStack.Server.Infrastructure.Http.Resilience;

namespace BodyStack.Server.Integrations.Suunto;

public sealed class SuuntoSleepExportClient : ISuuntoSleepExportClient
{
    private readonly HttpClient _http;

    public SuuntoSleepExportClient(HttpClient http)
    {
        _http = http;
    }

    public IObservable<HttpResponseMessage> GetSleepExportAsync(
        string sttAuthorization, 
        CancellationToken cancellationToken = default)
    {
        return Observable.FromAsync(async ct =>
        {
            var policy = ResiliencePolicyFactory.CreateStreamingPolicy();
            
            return await policy.ExecuteAsync(async innerCt =>
            {
                var req = new HttpRequestMessage(HttpMethod.Get, "/v1/sleep/export");
                AddHeaders(req, sttAuthorization);
                return await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, innerCt);
            }, ct);
        });
    }

    public IObservable<HttpResponseMessage> GetSleepStagesExportAsync(
        string sttAuthorization, 
        CancellationToken cancellationToken = default)
    {
        return Observable.FromAsync(async ct =>
        {
            var policy = ResiliencePolicyFactory.CreateStreamingPolicy();
            
            return await policy.ExecuteAsync(async innerCt =>
            {
                var req = new HttpRequestMessage(HttpMethod.Get, "/v1/sleepstages/export");
                AddHeaders(req, sttAuthorization);
                return await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, innerCt);
            }, ct);
        });
    }

    private static void AddHeaders(HttpRequestMessage req, string sttAuthorization)
    {
        req.Headers.TryAddWithoutValidation("sttauthorization", sttAuthorization);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-ndjson"));
        req.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        req.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        req.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
    }
}
