// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Invoices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Repairs;

public sealed record RepairOrderCacheKey(

    int Page,

    int PageSize,

    string SearchTerm,

    RepairOrderSortField SortBy,

    bool SortDescending,

    int FilterCustomerId,

    int FilterVehicleId,

    int FilterInvoiceId,

    RepairOrderStatus? FilterStatus);

public sealed class RepairOrderCacheEntry

{
    public required List<RepairOrderDto> RepairOrders { get; init; }

    public required int TotalCount { get; init; }

    public required DateTime ExpiresAt { get; init; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

public sealed class RepairOrderQueryCache

{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<RepairOrderCacheKey, RepairOrderCacheEntry> _store = new();

    public bool TryGet(RepairOrderCacheKey key, out RepairOrderCacheEntry? entry)

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

    public void Set(RepairOrderCacheKey key, List<RepairOrderDto> repairOrders, int totalCount)

    {
        _store[key] = new RepairOrderCacheEntry

        {
            RepairOrders = repairOrders,

            TotalCount = totalCount,

            ExpiresAt = DateTime.UtcNow.Add(Ttl)

        };

    }

    public void Invalidate() => _store.Clear();
}
