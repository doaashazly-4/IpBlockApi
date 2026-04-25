using System.Net;

namespace IpBlockApi.Helpers;

public static class IpAddressHelper
{
    public static bool IsValidIpFormat(string? value, out IPAddress? address)
    {
        address = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return IPAddress.TryParse(value.Trim(), out address);
    }
}
