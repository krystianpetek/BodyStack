namespace BodyStack.Server.Domain.Exceptions;

public class FitatuSessionNotFoundException : DomainException
{
    public string? FitatuUserId { get; }

    public FitatuSessionNotFoundException(string? fitatuUserId)
        : base("FITATU_SESSION_NOT_FOUND", $"Fitatu session not found for user {fitatuUserId ?? "unknown"}")
    {
        FitatuUserId = fitatuUserId;
    }
}
