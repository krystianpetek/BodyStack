using System.Reactive;

namespace BodyStack.Server.Integrations.Suunto;

/// <summary>
/// Client for exporting activity data from Suunto API using reactive streaming
/// </summary>
/// <remarks>
/// This client uses RX.NET (IObservable) for streaming HTTP responses,
/// enabling efficient memory usage and cancellation support.
/// </remarks>
public interface ISuuntoActivityExportClient
{
    /// <summary>
    /// Gets activity export data from Suunto API as an observable stream
    /// </summary>
    /// <param name="sttAuthorization">STT authorization token</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Observable containing HTTP response with activity data stream</returns>
    /// <exception cref="ArgumentException">Thrown when sttAuthorization is empty</exception>
    /// <exception cref="HttpRequestException">Thrown when HTTP request fails</exception>
    IObservable<HttpResponseMessage> GetActivityExportAsync(
        string sttAuthorization,
        CancellationToken cancellationToken = default);
}
