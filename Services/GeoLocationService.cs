using System.Net;
using System.Text;
using IpBlockApi.Models;
using IpBlockApi.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IpBlockApi.Services;

public sealed class GeoLocationService : IGeoLocationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeoLocationOptions _options;

    public GeoLocationService(IHttpClientFactory httpClientFactory, IOptions<GeoLocationOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<GeoIpLookupResult> LookupAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("GeoLocation");
        var provider = (_options.Provider ?? "ipapi").Trim();

        try
        {
            if (provider.Equals("ipgeolocation", StringComparison.OrdinalIgnoreCase))
                return await LookupIpGeoLocationAsync(client, ipAddress, cancellationToken).ConfigureAwait(false);

            return await LookupIpApiAsync(client, ipAddress, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new GeoIpLookupResult
            {
                Success = false,
                ErrorMessage = "Geolocation request timed out.",
                StatusCode = StatusCodes.Status504GatewayTimeout
            };
        }
        catch (HttpRequestException ex)
        {
            return new GeoIpLookupResult
            {
                Success = false,
                ErrorMessage = $"Geolocation HTTP error: {ex.Message}",
                StatusCode = StatusCodes.Status502BadGateway
            };
        }
    }

    private async Task<GeoIpLookupResult> LookupIpApiAsync(HttpClient client, string ipAddress, CancellationToken cancellationToken)
    {
        var baseUrl = _options.IpApiBaseUrl.TrimEnd('/');
        var sb = new StringBuilder($"{baseUrl}/{WebUtility.UrlEncode(ipAddress)}/json/");
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            sb.Append($"?key={WebUtility.UrlEncode(_options.ApiKey)}");

        using var response = await client.GetAsync(sb.ToString(), cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return new GeoIpLookupResult
            {
                Success = false,
                ErrorMessage = "Geolocation provider rate limit exceeded. Try again later.",
                StatusCode = StatusCodes.Status429TooManyRequests
            };
        }

        if (!response.IsSuccessStatusCode)
        {
            return new GeoIpLookupResult
            {
                Success = false,
                ErrorMessage = $"Geolocation provider returned {(int)response.StatusCode}.",
                StatusCode = StatusCodes.Status502BadGateway
            };
        }

        JObject token;
        try
        {
            token = JObject.Parse(body);
        }
        catch (JsonException)
        {
            return new GeoIpLookupResult
            {
                Success = false,
                ErrorMessage = "Invalid JSON returned by geolocation provider.",
                StatusCode = StatusCodes.Status502BadGateway
            };
        }

        if (token["error"]?.Value<bool>() == true)
        {
            var reason = token["reason"]?.Value<string>();
            var message = token["message"]?.Value<string>() ?? reason ?? "Unknown provider error.";
            var status = reason?.Equals("RateLimited", StringComparison.OrdinalIgnoreCase) == true
                ? StatusCodes.Status429TooManyRequests
                : StatusCodes.Status400BadRequest;
            return new GeoIpLookupResult { Success = false, ErrorMessage = message, StatusCode = status };
        }

        var dto = new IpLookupResultDto
        {
            IpAddress = token["ip"]?.Value<string>() ?? ipAddress,
            CountryCode = token["country_code"]?.Value<string>() ?? token["country"]?.Value<string>(),
            CountryName = token["country_name"]?.Value<string>(),
            Region = token["region"]?.Value<string>(),
            City = token["city"]?.Value<string>(),
            Isp = token["org"]?.Value<string>(),
            Organization = token["org"]?.Value<string>(),
            TimeZone = token["timezone"]?.Value<string>()
        };

        return new GeoIpLookupResult { Success = true, Data = dto };
    }

    private async Task<GeoIpLookupResult> LookupIpGeoLocationAsync(HttpClient client, string ipAddress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new GeoIpLookupResult
            {
                Success = false,
                ErrorMessage = "GeoLocation:ApiKey is required when Provider is ipgeolocation.",
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        var baseUrl = _options.IpGeoLocationBaseUrl.TrimEnd('/');
        var url =
            $"{baseUrl}/ipgeo?apiKey={WebUtility.UrlEncode(_options.ApiKey)}&ip={WebUtility.UrlEncode(ipAddress)}";

        using var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return new GeoIpLookupResult
            {
                Success = false,
                ErrorMessage = "Geolocation provider rate limit exceeded. Try again later.",
                StatusCode = StatusCodes.Status429TooManyRequests
            };
        }

        if (!response.IsSuccessStatusCode)
        {
            return new GeoIpLookupResult
            {
                Success = false,
                ErrorMessage = $"Geolocation provider returned {(int)response.StatusCode}.",
                StatusCode = StatusCodes.Status502BadGateway
            };
        }

        JObject token;
        try
        {
            token = JObject.Parse(body);
        }
        catch (JsonException)
        {
            return new GeoIpLookupResult
            {
                Success = false,
                ErrorMessage = "Invalid JSON returned by geolocation provider.",
                StatusCode = StatusCodes.Status502BadGateway
            };
        }

        if (token["message"] != null && token["country_code2"] == null)
        {
            return new GeoIpLookupResult
            {
                Success = false,
                ErrorMessage = token["message"]?.Value<string>() ?? "Provider error.",
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var dto = new IpLookupResultDto
        {
            IpAddress = token["ip"]?.Value<string>() ?? ipAddress,
            CountryCode = token["country_code2"]?.Value<string>(),
            CountryName = token["country_name"]?.Value<string>(),
            Region = token["state_prov"]?.Value<string>(),
            City = token["city"]?.Value<string>(),
            Isp = token["isp"]?.Value<string>(),
            Organization = token["organization"]?.Value<string>(),
            TimeZone = token["time_zone"]?["name"]?.Value<string>()
        };

        return new GeoIpLookupResult { Success = true, Data = dto };
    }
}
