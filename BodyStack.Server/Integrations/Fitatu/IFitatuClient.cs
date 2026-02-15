using System.Reactive;
using System.Text.Json;

namespace BodyStack.Server.Integrations.Fitatu;

/// <summary>
/// Client for interacting with the Fitatu API using reactive streaming
/// </summary>
/// <remarks>
/// This client uses RX.NET (IObservable) for streaming HTTP responses,
/// enabling efficient memory usage and cancellation support.
/// </remarks>
public interface IFitatuClient
{
    /// <summary>
    /// Authenticates with Fitatu API and returns login result as an observable
    /// </summary>
    /// <param name="username">Fitatu username</param>
    /// <param name="password">Fitatu password</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Observable containing the login result with token and refresh token</returns>
    /// <exception cref="ArgumentException">Thrown when username or password is empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when login response is invalid</exception>
    /// <exception cref="HttpRequestException">Thrown when HTTP request fails</exception>
    IObservable<FitatuLoginResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets diet and activity plan for a specific day as an observable
    /// </summary>
    /// <param name="userId">Fitatu user ID</param>
    /// <param name="date">Date to get plan for</param>
    /// <param name="token">Authentication token</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Observable containing the JSON document with diet and activity data</returns>
    /// <exception cref="ArgumentException">Thrown when userId or token is empty</exception>
    /// <exception cref="HttpRequestException">Thrown when HTTP request fails</exception>
    IObservable<JsonDocument> GetDietAndActivityPlanDayAsync(
        string userId,
        DateOnly date,
        string token,
        CancellationToken cancellationToken = default);
}
