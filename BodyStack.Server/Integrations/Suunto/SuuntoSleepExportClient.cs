using System.Net.Http.Headers;

namespace BodyStack.Server.Integrations.Suunto;

public sealed class SuuntoSleepExportClient : ISuuntoSleepExportClient
{
    private readonly HttpClient _http;

    public SuuntoSleepExportClient(HttpClient http)
    {
        _http = http;
    }

    public Task<HttpResponseMessage> GetSleepExportAsync(string sttAuthorization, CancellationToken cancellationToken = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/v1/sleep/export");
        AddHeaders(req, sttAuthorization);
        return _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    public Task<HttpResponseMessage> GetSleepStagesExportAsync(string sttAuthorization, CancellationToken cancellationToken = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/v1/sleepstages/export");
        AddHeaders(req, sttAuthorization);
        return _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
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
