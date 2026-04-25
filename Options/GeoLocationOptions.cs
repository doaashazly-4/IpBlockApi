namespace IpBlockApi.Options;

public class GeoLocationOptions
{
    public const string SectionName = "GeoLocation";

    /// <summary>ipapi or ipgeolocation</summary>
    public string Provider { get; set; } = "ipapi";

    public string ApiKey { get; set; } = "";

    public string IpApiBaseUrl { get; set; } = "https://ipapi.co";

    public string IpGeoLocationBaseUrl { get; set; } = "https://api.ipgeolocation.io";
}
