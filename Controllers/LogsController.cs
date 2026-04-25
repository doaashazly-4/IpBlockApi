using IpBlockApi.Helpers;
using IpBlockApi.Models;
using IpBlockApi.Models.Dtos;
using IpBlockApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IpBlockApi.Controllers;

[ApiController]
[Route("api/logs")]
public sealed class LogsController : ControllerBase
{
    private readonly IBlockedAttemptLogRepository _logs;

    public LogsController(IBlockedAttemptLogRepository logs)
    {
        _logs = logs;
    }

    [HttpGet("blocked-attempts")]
    [ProducesResponseType(typeof(PagedResult<BlockedAttemptLogEntry>), StatusCodes.Status200OK)]
    public ActionResult<PagedResult<BlockedAttemptLogEntry>> GetBlockedAttempts([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var (p, ps) = PaginationHelper.Normalize(page, pageSize);
        var pageItems = _logs.GetPage(p, ps, out var total);

        return Ok(new PagedResult<BlockedAttemptLogEntry>
        {
            Items = pageItems,
            Page = p,
            PageSize = ps,
            TotalCount = total
        });
    }
}
