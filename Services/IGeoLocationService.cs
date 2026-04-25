using IpBlockApi.Models;

namespace IpBlockApi.Services;

public interface IGeoLocationService
{
    Task<GeoIpLookupResult> LookupAsync(string ipAddress, CancellationToken cancellationToken = default);
}

public sealed class GeoIpLookupResult
{
    public bool Success { get; init; }
    public IpLookupResultDto? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public int? StatusCode { get; init; }
}
