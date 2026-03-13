// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Billings;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Billings;

public sealed record RepairTaskCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    RepairTaskSortField SortBy,
    System.Boolean SortDescending,
    System.Int32 FilterRepairOrderId,
    System.Int32 FilterEmployeeId,
    System.Int32 FilterServiceItemId,
    RepairOrderStatus? FilterStatus);

public sealed class RepairTaskCacheEntry
{
    public required List<RepairTaskDto> RepairTasks { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

public sealed class RepairTaskQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<RepairTaskCacheKey, RepairTaskCacheEntry> _store = new();

    public System.Boolean TryGet(RepairTaskCacheKey key, out RepairTaskCacheEntry? entry)
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

    public void Set(RepairTaskCacheKey key, List<RepairTaskDto> repairTasks, System.Int32 totalCount)
    {
        _store[key] = new RepairTaskCacheEntry
        {
            RepairTasks = repairTasks,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    public void Invalidate() => _store.Clear();
}

