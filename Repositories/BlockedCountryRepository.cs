using System.Collections.Concurrent;
using IpBlockApi.Helpers;
using IpBlockApi.Models;

namespace IpBlockApi.Repositories;

public sealed class BlockedCountryRepository : IBlockedCountryRepository
{
    private readonly ConcurrentDictionary<string, BlockedCountryEntry> _items = new(StringComparer.OrdinalIgnoreCase);

    public bool TryAdd(BlockedCountryEntry entry)
    {
        var key = CountryCodeHelper.Normalize(entry.CountryCode);
        var stored = entry with { CountryCode = key };
        return _items.TryAdd(key, stored);
    }

    public bool TryRemove(string countryCode)
    {
        var key = CountryCodeHelper.Normalize(countryCode);
        return _items.TryRemove(key, out _);
    }

    public bool Contains(string countryCode) =>
        _items.ContainsKey(CountryCodeHelper.Normalize(countryCode));

    public IReadOnlyList<BlockedCountryEntry> GetAll() =>
        _items.Values.OrderBy(x => x.CountryCode).ToList();
}
