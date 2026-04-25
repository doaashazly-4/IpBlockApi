using IpBlockApi.Models;

namespace IpBlockApi.Repositories;

public interface IBlockedCountryRepository
{
    bool TryAdd(BlockedCountryEntry entry);
    bool TryRemove(string countryCode);
    bool Contains(string countryCode);
    IReadOnlyList<BlockedCountryEntry> GetAll();
}
