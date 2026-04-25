using System.Globalization;

namespace IpBlockApi.Helpers;

public static class CountryCodeHelper
{
    public static string Normalize(string code) => code.Trim().ToUpperInvariant();

    public static bool IsValidIsoCountryCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 2)
            return false;

        try
        {
            _ = new RegionInfo(Normalize(code));
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static string GetEnglishName(string twoLetterCode)
    {
        var region = new RegionInfo(Normalize(twoLetterCode));
        return region.EnglishName;
    }
}
