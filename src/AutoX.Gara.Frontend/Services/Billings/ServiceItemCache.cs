using AutoX.Gara.Shared.Enums;
using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Protocol.Billings;

using System.Collections.Concurrent;

namespace AutoX.Gara.Frontend.Services.Billings;

public sealed record ServiceItemCacheKey(

    int Page,

    int PageSize,

    string SearchTerm,

    ServiceItemSortField SortBy,

    bool SortDescending,

    ServiceType? FilterType,

    decimal? FilterMinUnitPrice,

    decimal? FilterMaxUnitPrice);

public sealed class ServiceItemCacheEntry

{
    public required List<ServiceItemDto> ServiceItems { get; init; }

    public required int TotalCount { get; init; }

    public required DateTime ExpiresAt { get; init; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

/// <summary>

/// Thread-safe in-memory cache for service-item queries with 30-second TTL.

/// </summary>

public sealed class ServiceItemQueryCache

{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<ServiceItemCacheKey, ServiceItemCacheEntry> _store = new();

    public bool TryGet(ServiceItemCacheKey key, out ServiceItemCacheEntry? entry)

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

    public void Set(ServiceItemCacheKey key, List<ServiceItemDto> serviceItems, int totalCount)

    {
        _store[key] = new ServiceItemCacheEntry

        {
            ServiceItems = serviceItems,

            TotalCount = totalCount,

            ExpiresAt = DateTime.UtcNow.Add(Ttl)

        };

    }

    public void Invalidate() => _store.Clear();
}