// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Suppliers;
using System.Collections.Concurrent;

namespace AutoX.Gara.Frontend.Services.Suppliers;

/// <summary>
/// Cache key for supplier query parameters.
/// </summary>
public sealed record SupplierCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    SupplierSortField SortBy,
    System.Boolean SortDescending,
    AutoX.Gara.Domain.Enums.SupplierStatus FilterStatus,
    AutoX.Gara.Domain.Enums.Payments.PaymentTerms FilterPaymentTerms);

/// <summary>
/// Cache entry for supplier query results.
/// </summary>
public sealed class SupplierCacheEntry
{
    /// <summary>
    /// List of suppliers in this cache entry.
    /// </summary>
    public required System.Collections.Generic.List<SupplierDto> Suppliers { get; init; }

    /// <summary>
    /// Total count of suppliers matching the query.
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
/// Thread-safe in-memory cache for supplier queries with 30-second TTL.
/// Invalidates all cache on write operations to prevent stale data.
/// </summary>
public sealed class SupplierQueryCache : ISupplierQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<SupplierCacheKey, SupplierCacheEntry> _store = new();

    /// <summary>
    /// Attempts to retrieve a cached entry by key.
    /// </summary>
    public System.Boolean TryGet(SupplierCacheKey key, out SupplierCacheEntry? entry)
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
    public void Set(SupplierCacheKey key, System.Collections.Generic.List<SupplierDto> suppliers, System.Int32 totalCount)
    {
        _store[key] = new SupplierCacheEntry
        {
            Suppliers = suppliers,
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
/// Abstraction for supplier query cache.
/// </summary>
public interface ISupplierQueryCache
{
    System.Boolean TryGet(SupplierCacheKey key, out SupplierCacheEntry? entry);
    void Set(SupplierCacheKey key, System.Collections.Generic.List<SupplierDto> suppliers, System.Int32 totalCount);
    void Invalidate();
}
