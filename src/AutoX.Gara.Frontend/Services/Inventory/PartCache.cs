using AutoX.Gara.Shared.Enums;
using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Protocol.Inventory;

using System.Collections.Concurrent;

namespace AutoX.Gara.Frontend.Services.Inventory;

/// <summary>

/// Cache key for part query parameters.

/// </summary>

public sealed record PartCacheKey(

    int Page,

    int PageSize,

    string SearchTerm,

    PartSortField SortBy,

    bool SortDescending,

    int? FilterSupplierId,

    PartCategory? FilterCategory,

    bool? FilterInStock,

    bool? FilterDefective,

    bool? FilterExpired,

    bool? FilterDiscontinued);

/// <summary>

/// Cache entry containing part data and expiration time.

/// </summary>

public sealed class PartCacheEntry

{
    /// <summary>

    /// List of parts in this cache entry.

    /// </summary>

    public required List<PartDto> Parts { get; init; }

    /// <summary>

    /// Total count of parts matching the query.

    /// </summary>

    public required int TotalCount { get; init; }

    /// <summary>

    /// Expiration timestamp in UTC.

    /// </summary>

    public required DateTime ExpiresAt { get; init; }

    /// <summary>

    /// Checks if this entry has expired.

    /// </summary>

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

/// <summary>

/// Thread-safe in-memory cache for part queries with 30-second TTL.

/// Invalidates all cache on write operations to prevent stale data.

/// </summary>

public sealed class PartQueryCache : IPartQueryCache

{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<PartCacheKey, PartCacheEntry> _store = new();

    /// <summary>

    /// Attempts to retrieve a cached entry by key.

    /// </summary>

    public bool TryGet(PartCacheKey key, out PartCacheEntry? entry)

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

    /// <summary>

    /// Stores a cache entry with auto-expiration.

    /// </summary>

    public void Set(PartCacheKey key, List<PartDto> parts, int totalCount)

    {
        _store[key] = new PartCacheEntry

        {
            Parts = parts,

            TotalCount = totalCount,

            ExpiresAt = DateTime.UtcNow.Add(Ttl)

        };

    }

    /// <summary>

    /// Invalidates all cached entries.

    /// </summary>

    public void Invalidate() => _store.Clear();
}

/// <summary>

/// Abstraction for part query cache.

/// </summary>

public interface IPartQueryCache

{
    /// <summary>

    /// Attempts to retrieve a cached entry.

    /// </summary>

    bool TryGet(PartCacheKey key, out PartCacheEntry? entry);

    /// <summary>

    /// Stores a cache entry.

    /// </summary>

    void Set(PartCacheKey key, List<PartDto> parts, int totalCount);

    /// <summary>

    /// Invalidates all cached entries.

    /// </summary>

    void Invalidate();
}