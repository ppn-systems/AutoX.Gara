using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Protocol.Repairs;

using System.Collections.Concurrent;

using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Repairs;

public sealed record RepairOrderItemCacheKey(

    int Page,

    int PageSize,

    string SearchTerm,

    RepairOrderItemSortField SortBy,

    bool SortDescending,

    int FilterRepairOrderId,

    int FilterPartId);

public sealed class RepairOrderItemCacheEntry

{
    public required List<RepairOrderItemDto> Items { get; init; }

    public required int TotalCount { get; init; }

    public required DateTime ExpiresAt { get; init; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

public sealed class RepairOrderItemQueryCache

{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<RepairOrderItemCacheKey, RepairOrderItemCacheEntry> _store = new();

    public bool TryGet(RepairOrderItemCacheKey key, out RepairOrderItemCacheEntry? entry)

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

    public void Set(RepairOrderItemCacheKey key, List<RepairOrderItemDto> items, int totalCount)

    {
        _store[key] = new RepairOrderItemCacheEntry

        {
            Items = items,

            TotalCount = totalCount,

            ExpiresAt = DateTime.UtcNow.Add(Ttl)

        };

    }

    public void Invalidate() => _store.Clear();
}
