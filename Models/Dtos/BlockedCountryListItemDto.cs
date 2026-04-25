namespace IpBlockApi.Models.Dtos;

public sealed class BlockedCountryListItemDto
{
    public required string CountryCode { get; init; }
    public required string CountryName { get; init; }
    public string BlockKind { get; init; } = "Permanent";
    public DateTimeOffset? ExpiresAtUtc { get; init; }
}
