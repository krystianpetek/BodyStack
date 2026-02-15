using System.Reactive;

namespace BodyStack.Server.Integrations.Suunto;

/// <summary>
/// Client for exporting sleep data from Suunto API using reactive streaming
/// </summary>
/// <remarks>
/// This client uses RX.NET (IObservable) for streaming HTTP responses,
/// enabling efficient memory usage and cancellation support.
/// </remarks>
public interface ISuuntoSleepExportClient
{
    /// <summary>
    /// Gets sleep export data from Suunto API as an observable stream
    /// </summary>
    /// <param name="sttAuthorization">STT authorization token</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Observable containing HTTP response with sleep data stream</returns>
    /// <exception cref="ArgumentException">Thrown when sttAuthorization is empty</exception>
    /// <exception cref="HttpRequestException">Thrown when HTTP request fails</exception>
    IObservable<HttpResponseMessage> GetSleepExportAsync(
        string sttAuthorization,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sleep stages export data from Suunto API as an observable stream
    /// </summary>
    /// <param name="sttAuthorization">STT authorization token</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Observable containing HTTP response with sleep stages data stream</returns>
    /// <exception cref="ArgumentException">Thrown when sttAuthorization is empty</exception>
    /// <exception cref="HttpRequestException">Thrown when HTTP request fails</exception>
    IObservable<HttpResponseMessage> GetSleepStagesExportAsync(
        string sttAuthorization,
        CancellationToken cancellationToken = default);
}
