// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Invoices;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Repairs;

public sealed record RepairOrderCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    RepairOrderSortField SortBy,
    System.Boolean SortDescending,
    System.Int32 FilterCustomerId,
    System.Int32 FilterVehicleId,
    System.Int32 FilterInvoiceId,
    RepairOrderStatus? FilterStatus);

public sealed class RepairOrderCacheEntry
{
    public required List<RepairOrderDto> RepairOrders { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

public sealed class RepairOrderQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<RepairOrderCacheKey, RepairOrderCacheEntry> _store = new();

    public System.Boolean TryGet(RepairOrderCacheKey key, out RepairOrderCacheEntry? entry)
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

    public void Set(RepairOrderCacheKey key, List<RepairOrderDto> repairOrders, System.Int32 totalCount)
    {
        _store[key] = new RepairOrderCacheEntry
        {
            RepairOrders = repairOrders,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    public void Invalidate() => _store.Clear();
}

