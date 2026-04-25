namespace IpBlockApi.Models.Dtos;

public sealed class CheckBlockResponseDto
{
    public required string IpAddress { get; init; }
    public string? CountryCode { get; init; }
    public string? CountryName { get; init; }
    public bool IsBlocked { get; init; }
}
