// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Repairs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Invoices;

public sealed record RepairTaskCacheKey(

    int Page,

    int PageSize,

    string SearchTerm,

    RepairTaskSortField SortBy,

    bool SortDescending,

    int FilterRepairOrderId,

    int FilterEmployeeId,

    int FilterServiceItemId,

    RepairOrderStatus? FilterStatus);

public sealed class RepairTaskCacheEntry

{
    public required List<RepairTaskDto> RepairTasks { get; init; }

    public required int TotalCount { get; init; }

    public required DateTime ExpiresAt { get; init; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

public sealed class RepairTaskQueryCache

{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<RepairTaskCacheKey, RepairTaskCacheEntry> _store = new();

    public bool TryGet(RepairTaskCacheKey key, out RepairTaskCacheEntry? entry)

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

    public void Set(RepairTaskCacheKey key, List<RepairTaskDto> repairTasks, int totalCount)

    {
        _store[key] = new RepairTaskCacheEntry

        {
            RepairTasks = repairTasks,

            TotalCount = totalCount,

            ExpiresAt = DateTime.UtcNow.Add(Ttl)

        };

    }

    public void Invalidate() => _store.Clear();
}
