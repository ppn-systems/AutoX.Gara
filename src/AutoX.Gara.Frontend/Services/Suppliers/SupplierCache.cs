using AutoX.Gara.Contracts.Enums;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Contracts.Suppliers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
namespace AutoX.Gara.Frontend.Services.Suppliers;
/// <summary>
/// Cache key for supplier query parameters.
/// </summary>
public sealed record SupplierCacheKey(
    int Page,
    int PageSize,
    string SearchTerm,
    SupplierSortField SortBy,
    bool SortDescending,
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
    public required List<SupplierDto> Suppliers { get; init; }
    /// <summary>
    /// Total count of suppliers matching the query.
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
/// Thread-safe in-memory cache for supplier queries with 30-second TTL.
/// Invalidates all cache on write operations to prevent stale data.
/// </summary>
public sealed class SupplierQueryCache : ISupplierQueryCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<SupplierCacheKey, SupplierCacheEntry> _store = new();
    /// <summary>
    /// Attempts to retrieve a cached entry by key.
    /// </summary>
    public bool TryGet(SupplierCacheKey key, out SupplierCacheEntry? entry)
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
    public void Set(SupplierCacheKey key, List<SupplierDto> suppliers, int totalCount)
    {
        _store[key] = new SupplierCacheEntry
        {
            Suppliers = suppliers,
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
/// Abstraction for supplier query cache.
/// </summary>
public interface ISupplierQueryCache
{
    bool TryGet(SupplierCacheKey key, out SupplierCacheEntry? entry);
    void Set(SupplierCacheKey key, List<SupplierDto> suppliers, int totalCount);
    void Invalidate();
}


