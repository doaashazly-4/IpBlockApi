using System.Net;
using IpBlockApi.Helpers;
using IpBlockApi.Models;
using IpBlockApi.Models.Dtos;
using IpBlockApi.Repositories;
using IpBlockApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IpBlockApi.Controllers;

[ApiController]
[Route("api/ip")]
public sealed class IpController : ControllerBase
{
    private readonly IGeoLocationService _geoLocation;
    private readonly IIpClientInfoProvider _ipClientInfo;
    private readonly ICountryBlockPolicy _blockPolicy;
    private readonly IBlockedAttemptLogRepository _logs;

    public IpController(
        IGeoLocationService geoLocation,
        IIpClientInfoProvider ipClientInfo,
        ICountryBlockPolicy blockPolicy,
        IBlockedAttemptLogRepository logs)
    {
        _geoLocation = geoLocation;
        _ipClientInfo = ipClientInfo;
        _blockPolicy = blockPolicy;
        _logs = logs;
    }

    [HttpGet("lookup")]
    [ProducesResponseType(typeof(IpLookupResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Lookup([FromQuery] string? ipAddress, CancellationToken cancellationToken)
    {
        string? targetIp = ipAddress;
        if (string.IsNullOrWhiteSpace(targetIp))
        {
            targetIp = _ipClientInfo.GetClientIpAddress(HttpContext);
            if (string.IsNullOrWhiteSpace(targetIp))
                return BadRequest(new { message = "Could not determine caller IP address. Provide ipAddress query parameter." });
        }

        if (!IpAddressHelper.IsValidIpFormat(targetIp, out var parsed))
            return BadRequest(new { message = "Invalid IP address format.", ipAddress = targetIp });

        var normalized = parsed!.ToString();
        var result = await _geoLocation.LookupAsync(normalized, cancellationToken).ConfigureAwait(false);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? StatusCodes.Status502BadGateway, new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpGet("check-block")]
    [ProducesResponseType(typeof(CheckBlockResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CheckBlock(CancellationToken cancellationToken)
    {
        var ip = _ipClientInfo.GetClientIpAddress(HttpContext);
        if (string.IsNullOrWhiteSpace(ip))
            return BadRequest(new { message = "Could not determine caller IP address." });

        if (!IpAddressHelper.IsValidIpFormat(ip, out var parsed))
            return BadRequest(new { message = "Caller IP address is not a valid IP format.", ipAddress = ip });

        var normalized = parsed!.ToString();
        var ua = Request.Headers.UserAgent.ToString();

        var lookup = await _geoLocation.LookupAsync(normalized, cancellationToken).ConfigureAwait(false);
        if (!lookup.Success)
        {
            _logs.Append(new BlockedAttemptLogEntry
            {
                IpAddress = normalized,
                TimestampUtc = DateTimeOffset.UtcNow,
                CountryCode = null,
                IsBlocked = false,
                UserAgent = ua,
                ErrorMessage = lookup.ErrorMessage
            });

            return StatusCode(lookup.StatusCode ?? StatusCodes.Status502BadGateway, new { message = lookup.ErrorMessage });
        }

        var country = lookup.Data?.CountryCode;
        var blocked = _blockPolicy.IsCountryBlocked(country);

        _logs.Append(new BlockedAttemptLogEntry
        {
            IpAddress = normalized,
            TimestampUtc = DateTimeOffset.UtcNow,
            CountryCode = country,
            IsBlocked = blocked,
            UserAgent = ua,
            ErrorMessage = null
        });

        return Ok(new CheckBlockResponseDto
        {
            IpAddress = normalized,
            CountryCode = lookup.Data?.CountryCode,
            CountryName = lookup.Data?.CountryName,
            IsBlocked = blocked
        });
    }
}
