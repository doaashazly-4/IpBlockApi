namespace IpBlockApi.Models;

public sealed class BlockedAttemptLogEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string IpAddress { get; init; }
    public DateTimeOffset TimestampUtc { get; init; }
    public string? CountryCode { get; init; }
    public bool IsBlocked { get; init; }
    public string? UserAgent { get; init; }
    public string? ErrorMessage { get; init; }
}
