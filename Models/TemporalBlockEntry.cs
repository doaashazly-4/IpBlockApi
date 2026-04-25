namespace IpBlockApi.Models;

public sealed record TemporalBlockEntry(
    string CountryCode,
    string CountryName,
    DateTimeOffset BlockedAtUtc,
    DateTimeOffset ExpiresAtUtc);
