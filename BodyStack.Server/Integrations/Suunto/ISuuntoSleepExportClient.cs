namespace BodyStack.Server.Integrations.Suunto;

public interface ISuuntoSleepExportClient
{
    Task<HttpResponseMessage> GetSleepExportAsync(string sttAuthorization, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> GetSleepStagesExportAsync(string sttAuthorization, CancellationToken cancellationToken = default);
}
