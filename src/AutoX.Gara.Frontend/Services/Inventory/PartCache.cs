// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Inventory;
using System.Collections.Concurrent;

namespace AutoX.Gara.Frontend.Services.Inventory;

/// <summary>
/// Cache key for part query parameters.
/// </summary>
public sealed record PartCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    PartSortField SortBy,
    System.Boolean SortDescending,
    System.Int32? FilterSupplierId,
    PartCategory? FilterCategory,
    System.Boolean? FilterInStock,
    System.Boolean? FilterDefective,
    System.Boolean? FilterExpired,
    System.Boolean? FilterDiscontinued);

/// <summary>
/// Cache entry containing part data and expiration time.
/// </summary>
public sealed class PartCacheEntry
{
    /// <summary>
    /// List of parts in this cache entry.
    /// </summary>
    public required System.Collections.Generic.List<PartDto> Parts { get; init; }

    /// <summary>
    /// Total count of parts matching the query.
    /// </summary>
    public required System.Int32 TotalCount { get; init; }

    /// <summary>
    /// Expiration timestamp in UTC.
    /// </summary>
    public required System.DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Checks if this entry has expired.
    /// </summary>
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

/// <summary>
/// Thread-safe in-memory cache for part queries with 30-second TTL.
/// Invalidates all cache on write operations to prevent stale data.
/// </summary>
public sealed class PartQueryCache : IPartQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<PartCacheKey, PartCacheEntry> _store = new();

    /// <summary>
    /// Attempts to retrieve a cached entry by key.
    /// </summary>
    public System.Boolean TryGet(PartCacheKey key, out PartCacheEntry? entry)
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
    public void Set(PartCacheKey key, System.Collections.Generic.List<PartDto> parts, System.Int32 totalCount)
    {
        _store[key] = new PartCacheEntry
        {
            Parts = parts,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
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
    System.Boolean TryGet(PartCacheKey key, out PartCacheEntry? entry);

    /// <summary>
    /// Stores a cache entry.
    /// </summary>
    void Set(PartCacheKey key, System.Collections.Generic.List<PartDto> parts, System.Int32 totalCount);

    /// <summary>
    /// Invalidates all cached entries.
    /// </summary>
    void Invalidate();
}