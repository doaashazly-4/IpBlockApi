namespace IpBlockApi.Models;

public sealed class IpLookupResultDto
{
    public required string IpAddress { get; init; }
    public string? CountryCode { get; init; }
    public string? CountryName { get; init; }
    public string? Region { get; init; }
    public string? City { get; init; }
    public string? Isp { get; init; }
    public string? Organization { get; init; }
    public string? TimeZone { get; init; }
}
