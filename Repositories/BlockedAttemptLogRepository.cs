using IpBlockApi.Models;

namespace IpBlockApi.Repositories;

public sealed class BlockedAttemptLogRepository : IBlockedAttemptLogRepository
{
    private readonly List<BlockedAttemptLogEntry> _logs = [];
    private readonly object _sync = new();

    public void Append(BlockedAttemptLogEntry entry)
    {
        lock (_sync)
        {
            _logs.Add(entry);
        }
    }

    public IReadOnlyList<BlockedAttemptLogEntry> GetPage(int page, int pageSize, out int totalCount)
    {
        lock (_sync)
        {
            totalCount = _logs.Count;
            var ordered = _logs.OrderByDescending(x => x.TimestampUtc).ToList();
            var skip = (page - 1) * pageSize;
            return ordered.Skip(skip).Take(pageSize).ToList();
        }
    }
}
