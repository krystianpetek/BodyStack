namespace BodyStack.Server.Integrations.Suunto;

public interface ISuuntoActivityExportClient
{
    Task<HttpResponseMessage> GetActivityExportAsync(string sttAuthorization, CancellationToken cancellationToken = default);
}
