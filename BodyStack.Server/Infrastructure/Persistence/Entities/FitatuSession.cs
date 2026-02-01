namespace BodyStack.Server.Infrastructure.Persistence.Entities;

public sealed class FitatuSession
{
    public Guid Id { get; set; }
    public required string FitatuUserId { get; set; }
    public required string TokenProtected { get; set; }
    public required string RefreshTokenProtected { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
