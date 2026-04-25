using System.Net;

namespace IpBlockApi.Services;

public sealed class IpClientInfoProvider : IIpClientInfoProvider
{
    public string? GetClientIpAddress(HttpContext httpContext)
    {
        var forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            var candidate = forwarded.Split(',').Select(s => s.Trim()).FirstOrDefault(s => !string.IsNullOrEmpty(s));
            if (!string.IsNullOrEmpty(candidate))
                return NormalizeIpString(candidate);
        }

        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(realIp))
            return NormalizeIpString(realIp.Trim());

        return httpContext.Connection.RemoteIpAddress is { } addr
            ? NormalizeIpString(addr.ToString())
            : null;
    }

    private static string NormalizeIpString(string value)
    {
        if (IPAddress.TryParse(value, out var ip))
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && ip.IsIPv4MappedToIPv6)
                return ip.MapToIPv4().ToString();
        }

        return value;
    }
}
