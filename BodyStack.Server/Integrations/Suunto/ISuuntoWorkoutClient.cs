using System.Reactive;
using System.Text.Json;

namespace BodyStack.Server.Integrations.Suunto;

/// <summary>
/// Client for fetching workout data from Suunto API
/// </summary>
public interface ISuuntoWorkoutClient
{
    /// <summary>
    /// Fetches workouts from Suunto API
    /// </summary>
    IObservable<JsonDocument> GetWorkoutsAsync(
        string sttAuthorization, 
        CancellationToken cancellationToken = default);
}
