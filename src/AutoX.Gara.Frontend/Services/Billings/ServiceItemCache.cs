// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Billings;
using System.Collections.Concurrent;

namespace AutoX.Gara.Frontend.Services.Billings;

public sealed record ServiceItemCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    ServiceItemSortField SortBy,
    System.Boolean SortDescending,
    ServiceType? FilterType,
    System.Decimal? FilterMinUnitPrice,
    System.Decimal? FilterMaxUnitPrice);

public sealed class ServiceItemCacheEntry
{
    public required System.Collections.Generic.List<ServiceItemDto> ServiceItems { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

/// <summary>
/// Thread-safe in-memory cache for service-item queries with 30-second TTL.
/// </summary>
public sealed class ServiceItemQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<ServiceItemCacheKey, ServiceItemCacheEntry> _store = new();

    public System.Boolean TryGet(ServiceItemCacheKey key, out ServiceItemCacheEntry? entry)
    {
        if (_store.TryGetValue(key, out entry) && !entry.IsExpired)
        {
            return true;
        }

        if (entry is not null)
        {
            _store.TryRemove(key, out _);
        }

        entry = null;
        return false;
    }

    public void Set(ServiceItemCacheKey key, System.Collections.Generic.List<ServiceItemDto> serviceItems, System.Int32 totalCount)
    {
        _store[key] = new ServiceItemCacheEntry
        {
            ServiceItems = serviceItems,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    public void Invalidate() => _store.Clear();
}
