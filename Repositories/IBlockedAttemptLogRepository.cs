using IpBlockApi.Models;

namespace IpBlockApi.Repositories;

public interface IBlockedAttemptLogRepository
{
    void Append(BlockedAttemptLogEntry entry);
    IReadOnlyList<BlockedAttemptLogEntry> GetPage(int page, int pageSize, out int totalCount);
}
