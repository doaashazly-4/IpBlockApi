namespace IpBlockApi.Services;

public interface IIpClientInfoProvider
{
    string? GetClientIpAddress(HttpContext httpContext);
}
