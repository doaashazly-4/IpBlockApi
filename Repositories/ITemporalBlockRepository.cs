using IpBlockApi.Models;

namespace IpBlockApi.Repositories;

public interface ITemporalBlockRepository
{
    bool TryAdd(TemporalBlockEntry entry);
    bool HasActiveTemporalBlock(string countryCode);
    TemporalBlockEntry? GetActive(string countryCode);
    void RemoveExpired();
    IReadOnlyList<TemporalBlockEntry> GetActiveAll();
}
