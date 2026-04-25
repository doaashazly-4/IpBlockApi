using IpBlockApi.Helpers;
using IpBlockApi.Repositories;

namespace IpBlockApi.Services;

public sealed class CountryBlockPolicy : ICountryBlockPolicy
{
    private readonly IBlockedCountryRepository _blockedCountries;
    private readonly ITemporalBlockRepository _temporalBlocks;

    public CountryBlockPolicy(IBlockedCountryRepository blockedCountries, ITemporalBlockRepository temporalBlocks)
    {
        _blockedCountries = blockedCountries;
        _temporalBlocks = temporalBlocks;
    }

    public bool IsCountryBlocked(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return false;

        var code = CountryCodeHelper.Normalize(countryCode);
        return _blockedCountries.Contains(code) || _temporalBlocks.HasActiveTemporalBlock(code);
    }
}
