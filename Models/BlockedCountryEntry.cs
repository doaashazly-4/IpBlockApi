namespace IpBlockApi.Models;

public sealed record BlockedCountryEntry(
    string CountryCode,
    string CountryName,
    DateTimeOffset BlockedAtUtc);
