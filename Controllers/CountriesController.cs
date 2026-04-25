using IpBlockApi.Helpers;
using IpBlockApi.Models;
using IpBlockApi.Models.Dtos;
using IpBlockApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IpBlockApi.Controllers;

[ApiController]
[Route("api/countries")]
public sealed class CountriesController : ControllerBase
{
    private readonly IBlockedCountryRepository _blockedCountries;
    private readonly ITemporalBlockRepository _temporalBlocks;

    public CountriesController(IBlockedCountryRepository blockedCountries, ITemporalBlockRepository temporalBlocks)
    {
        _blockedCountries = blockedCountries;
        _temporalBlocks = temporalBlocks;
    }

    [HttpPost("block")]
    [ProducesResponseType(typeof(BlockedCountryEntry), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<BlockedCountryEntry> BlockCountry([FromBody] BlockCountryRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var code = CountryCodeHelper.Normalize(request.CountryCode);
        if (!CountryCodeHelper.IsValidIsoCountryCode(code))
            return BadRequest(new { message = "Invalid country code.", countryCode = request.CountryCode });

        if (_blockedCountries.Contains(code))
            return Conflict(new { message = "Country is already blocked.", countryCode = code });

        var name = CountryCodeHelper.GetEnglishName(code);
        var entry = new BlockedCountryEntry(code, name, DateTimeOffset.UtcNow);
        if (!_blockedCountries.TryAdd(entry))
            return Conflict(new { message = "Country is already blocked.", countryCode = code });

        return CreatedAtAction(nameof(GetBlockedCountries), new { }, entry);
    }

    [HttpDelete("block/{countryCode}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UnblockCountry([FromRoute] string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return NotFound();

        if (!_blockedCountries.TryRemove(countryCode))
            return NotFound(new { message = "Country is not in the permanent blocked list.", countryCode });

        return NoContent();
    }

    [HttpGet("blocked")]
    [ProducesResponseType(typeof(PagedResult<BlockedCountryListItemDto>), StatusCodes.Status200OK)]
    public ActionResult<PagedResult<BlockedCountryListItemDto>> GetBlockedCountries(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? search)
    {
        var (p, ps) = PaginationHelper.Normalize(page, pageSize);
        var query = search?.Trim();

        var permanent = _blockedCountries.GetAll()
            .Select(x => new BlockedCountryListItemDto
            {
                CountryCode = x.CountryCode,
                CountryName = x.CountryName,
                BlockKind = "Permanent",
                ExpiresAtUtc = null
            });

        var temporal = _temporalBlocks.GetActiveAll()
            .Select(x => new BlockedCountryListItemDto
            {
                CountryCode = x.CountryCode,
                CountryName = x.CountryName,
                BlockKind = "Temporary",
                ExpiresAtUtc = x.ExpiresAtUtc
            });

        var permanentCodes = new HashSet<string>(permanent.Select(p => p.CountryCode), StringComparer.OrdinalIgnoreCase);
        var combined = permanent
            .Concat(temporal.Where(t => !permanentCodes.Contains(t.CountryCode)))
            .ToList();

        if (!string.IsNullOrEmpty(query))
        {
            combined = combined
                .Where(x =>
                    x.CountryCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    x.CountryName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        combined = combined
            .OrderBy(x => x.CountryCode)
            .ThenBy(x => x.BlockKind)
            .ToList();

        var total = combined.Count;
        var items = combined.Skip((p - 1) * ps).Take(ps).ToList();

        return Ok(new PagedResult<BlockedCountryListItemDto>
        {
            Items = items,
            Page = p,
            PageSize = ps,
            TotalCount = total
        });
    }

    [HttpPost("temporal-block")]
    [ProducesResponseType(typeof(TemporalBlockEntry), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<TemporalBlockEntry> TemporalBlock([FromBody] TemporalBlockRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var code = CountryCodeHelper.Normalize(request.CountryCode);
        if (!CountryCodeHelper.IsValidIsoCountryCode(code))
            return BadRequest(new { message = "Invalid country code.", countryCode = request.CountryCode });

        if (request.DurationMinutes is < 1 or > 1440)
            return BadRequest(new { message = "durationMinutes must be between 1 and 1440.", durationMinutes = request.DurationMinutes });

        var name = CountryCodeHelper.GetEnglishName(code);
        var now = DateTimeOffset.UtcNow;
        var entry = new TemporalBlockEntry(code, name, now, now.AddMinutes(request.DurationMinutes));

        if (!_temporalBlocks.TryAdd(entry))
            return Conflict(new { message = "Country is already temporarily blocked.", countryCode = code });

        return CreatedAtAction(nameof(GetBlockedCountries), new { }, entry);
    }
}
