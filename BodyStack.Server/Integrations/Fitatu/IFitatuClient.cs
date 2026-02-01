using System.Text.Json;

namespace BodyStack.Server.Integrations.Fitatu;

public interface IFitatuClient
{
    Task<FitatuLoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

    Task<JsonDocument> GetDietAndActivityPlanDayAsync(
        string userId,
        DateOnly date,
        string token,
        CancellationToken cancellationToken = default);
}
