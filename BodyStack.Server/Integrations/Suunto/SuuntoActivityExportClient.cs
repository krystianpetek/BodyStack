using System.Net.Http.Headers;

namespace BodyStack.Server.Integrations.Suunto;

public sealed class SuuntoActivityExportClient : ISuuntoActivityExportClient
{
    private readonly HttpClient _http;

    public SuuntoActivityExportClient(HttpClient http)
    {
        _http = http;
    }

    public Task<HttpResponseMessage> GetActivityExportAsync(string sttAuthorization, CancellationToken cancellationToken = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/v1/activity/export");
        req.Headers.TryAddWithoutValidation("sttauthorization", sttAuthorization);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-ndjson"));
        req.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        req.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        req.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

        return _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }
}
