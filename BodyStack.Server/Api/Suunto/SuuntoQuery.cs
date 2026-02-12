namespace BodyStack.Server.Api.Suunto;

public sealed record SuuntoDailyQuery(string? From, string? To, int? TtlMinutes);
