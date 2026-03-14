// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Billings;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Repairs;

public sealed record RepairOrderItemCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    RepairOrderItemSortField SortBy,
    System.Boolean SortDescending,
    System.Int32 FilterRepairOrderId,
    System.Int32 FilterPartId);

public sealed class RepairOrderItemCacheEntry
{
    public required List<RepairOrderItemDto> Items { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

public sealed class RepairOrderItemQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<RepairOrderItemCacheKey, RepairOrderItemCacheEntry> _store = new();

    public System.Boolean TryGet(RepairOrderItemCacheKey key, out RepairOrderItemCacheEntry? entry)
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

    public void Set(RepairOrderItemCacheKey key, List<RepairOrderItemDto> items, System.Int32 totalCount)
    {
        _store[key] = new RepairOrderItemCacheEntry
        {
            Items = items,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    public void Invalidate() => _store.Clear();
}

