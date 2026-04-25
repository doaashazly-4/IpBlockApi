using System.Collections.Concurrent;
using IpBlockApi.Helpers;
using IpBlockApi.Models;

namespace IpBlockApi.Repositories;

public sealed class TemporalBlockRepository : ITemporalBlockRepository
{
    private readonly ConcurrentDictionary<string, TemporalBlockEntry> _items = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _addGate = new();

    public bool TryAdd(TemporalBlockEntry entry)
    {
        var key = CountryCodeHelper.Normalize(entry.CountryCode);
        var normalized = entry with { CountryCode = key };

        lock (_addGate)
        {
            RemoveExpiredUnsafe();
            if (_items.TryGetValue(key, out var existing) && DateTimeOffset.UtcNow < existing.ExpiresAtUtc)
                return false;

            _items[key] = normalized;
            return true;
        }
    }

    public bool HasActiveTemporalBlock(string countryCode)
    {
        var key = CountryCodeHelper.Normalize(countryCode);
        if (!_items.TryGetValue(key, out var entry))
            return false;

        if (DateTimeOffset.UtcNow >= entry.ExpiresAtUtc)
        {
            _items.TryRemove(key, out _);
            return false;
        }

        return true;
    }

    public TemporalBlockEntry? GetActive(string countryCode)
    {
        var key = CountryCodeHelper.Normalize(countryCode);
        if (!_items.TryGetValue(key, out var entry))
            return null;

        if (DateTimeOffset.UtcNow >= entry.ExpiresAtUtc)
        {
            _items.TryRemove(key, out _);
            return null;
        }

        return entry;
    }

    public void RemoveExpired()
    {
        lock (_addGate)
            RemoveExpiredUnsafe();
    }

    private void RemoveExpiredUnsafe()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var kv in _items.ToArray())
        {
            if (now >= kv.Value.ExpiresAtUtc)
                _items.TryRemove(kv.Key, out _);
        }
    }

    public IReadOnlyList<TemporalBlockEntry> GetActiveAll()
    {
        RemoveExpired();
        return _items.Values.OrderBy(x => x.CountryCode).ToList();
    }
}
