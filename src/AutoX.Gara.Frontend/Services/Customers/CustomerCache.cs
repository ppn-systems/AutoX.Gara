// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Customers;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Customers;

/// <summary>
/// Key duy nh?t cho m?t t?p tham s? truy vụn.
/// C# record t? sinh <c>Equals</c> + <c>GetHashCode</c> dúng —
/// dùng du?c tr?c ti?p làm key c?a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
public sealed record CustomerCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    CustomerSortField SortBy,
    System.Boolean SortDescending,
    CustomerType FilterType,
    MembershipLevel FilterMembership);

/// <summary>
/// M?t entry trong cache g?m d? li?u và th?i di?m h?t h?n.
/// </summary>
public sealed class CustomerCacheEntry
{
    public required List<CustomerDto> Customers { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }

    /// <summary>
    /// <c>true</c> khi entry dã quá TTL và không còn h?p l?.
    /// </summary>
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

/// <summary>
/// In-memory cache thread-safe vụi TTL 30 giây.
/// <para>
/// Vòng đổi cache: mới (page, pageSize, search, filter, sort) là m?t entry d?c l?p.
/// Khi user th?c hi?n write operation, toàn b? cache b? xóa d? tránh stale data.
/// </para>
/// </summary>
public sealed class CustomerQueryCache : ICustomerQueryCache
{
    /// <summary>TTL 30 giây — d? d? tránh duplicate request khi navigate, d? ng?n d? data không stale.</summary>
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<CustomerCacheKey, CustomerCacheEntry> _store = new();

    /// <inheritdoc/>
    public System.Boolean TryGet(CustomerCacheKey key, out CustomerCacheEntry? entry)
    {
        if (_store.TryGetValue(key, out entry) && !entry.IsExpired)
        {
            return true;
        }

        // Entry t?n Tải nhung dã h?t h?n ? xóa luôn d? tránh tích luy b? nh?
        if (entry is not null)
        {
            _store.TryRemove(key, out _);
        }

        entry = null;
        return false;
    }

    /// <inheritdoc/>
    public void Set(CustomerCacheKey key, List<CustomerDto> customers, System.Int32 totalCount)
    {
        _store[key] = new CustomerCacheEntry
        {
            Customers = customers,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    /// <inheritdoc/>
    public void Invalidate() => _store.Clear();
}
