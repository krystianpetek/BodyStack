namespace BodyStack.Server.Integrations.Fitatu;

public sealed class FitatuOptions
{
    public required string BaseUrl { get; init; }
    public required string ApiKey { get; init; }
    public string? ApiSecret { get; init; }
}
