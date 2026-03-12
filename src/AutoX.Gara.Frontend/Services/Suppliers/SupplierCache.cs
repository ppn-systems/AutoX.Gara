// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Suppliers;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Suppliers;

/// <summary>
/// Key duy nhất cho một tập tham số truy vấn nhà cung cấp.
/// C# record tự sinh <c>Equals</c> + <c>GetHashCode</c> đúng —
/// dùng được trực tiếp làm key của <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
public sealed record SupplierCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    SupplierSortField SortBy,
    System.Boolean SortDescending,
    SupplierStatus FilterStatus,
    PaymentTerms FilterPaymentTerms);

/// <summary>
/// Một entry trong cache gồm dữ liệu và thời điểm hết hạn.
/// </summary>
public sealed class SupplierCacheEntry
{
    public required List<SupplierDto> Suppliers { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }

    /// <summary>
    /// <c>true</c> khi entry đã quá TTL và không còn hợp lệ.
    /// </summary>
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

/// <summary>
/// In-memory cache thread-safe với TTL 30 giây.
/// <para>
/// Vòng đời cache: mỗi (page, pageSize, search, filter, sort) là một entry độc lập.
/// Khi user thực hiện write operation, toàn bộ cache bị xóa để tránh stale data.
/// </para>
/// </summary>
public sealed class SupplierQueryCache : ISupplierQueryCache
{
    /// <summary>TTL 30 giây — đủ để tránh duplicate request khi navigate, đủ ngắn để data không stale.</summary>
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<SupplierCacheKey, SupplierCacheEntry> _store = new();

    /// <inheritdoc/>
    public System.Boolean TryGet(SupplierCacheKey key, out SupplierCacheEntry? entry)
    {
        if (_store.TryGetValue(key, out entry) && !entry.IsExpired)
        {
            return true;
        }

        // Entry tồn tại nhưng đã hết hạn → xóa luôn để tránh tích lũy bộ nhớ
        if (entry is not null)
        {
            _store.TryRemove(key, out _);
        }

        entry = null;
        return false;
    }

    /// <inheritdoc/>
    public void Set(SupplierCacheKey key, List<SupplierDto> suppliers, System.Int32 totalCount)
    {
        _store[key] = new SupplierCacheEntry
        {
            Suppliers = suppliers,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    /// <inheritdoc/>
    public void Invalidate() => _store.Clear();
}
