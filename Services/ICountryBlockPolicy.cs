namespace IpBlockApi.Services;

public interface ICountryBlockPolicy
{
    bool IsCountryBlocked(string? countryCode);
}
