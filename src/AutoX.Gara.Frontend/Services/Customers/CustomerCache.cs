using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Customers;

using AutoX.Gara.Frontend.Abstractions;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Protocol.Customers;

using System.Collections.Concurrent;

using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Customers;

/// <summary>

/// Key duy nh?t cho m?t t?p tham s? truy v?n.

/// C# record t? sinh <c>Equals</c> + <c>GetHashCode</c> d�ng �

/// d�ng du?c tr?c ti?p l�m key c?a <see cref="ConcurrentDictionary{TKey,TValue}"/>.

/// </summary>

public sealed record CustomerCacheKey(

    int Page,

    int PageSize,

    string SearchTerm,

    CustomerSortField SortBy,

    bool SortDescending,

    CustomerType FilterType,

    MembershipLevel FilterMembership);

/// <summary>

/// M?t entry trong cache g?m d? li?u v� th?i di?m h?t h?n.

/// </summary>

public sealed class CustomerCacheEntry

{
    public required List<CustomerDto> Customers { get; init; }

    public required int TotalCount { get; init; }

    public required DateTime ExpiresAt { get; init; }

    /// <summary>

    /// <c>true</c> khi entry d� qu� TTL v� kh�ng c�n h?p l?.

    /// </summary>

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

/// <summary>

/// In-memory cache thread-safe v?i TTL 30 gi�y.

/// <para>

/// V�ng d?i cache: m?i (page, pageSize, search, filter, sort) l� m?t entry d?c l?p.

/// Khi user th?c hi?n write operation, to�n b? cache b? x�a d? tr�nh stale data.

/// </para>

/// </summary>

public sealed class CustomerQueryCache : ICustomerQueryCache

{
    /// <summary>TTL 30 gi�y � d? d? tr�nh duplicate request khi navigate, d? ng?n d? data kh�ng stale.</summary>

    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<CustomerCacheKey, CustomerCacheEntry> _store = new();

    /// <inheritdoc/>

    public bool TryGet(CustomerCacheKey key, out CustomerCacheEntry? entry)

    {
        if (_store.TryGetValue(key, out entry) && !entry.IsExpired)

        {
            return true;

        }

        // Entry t?n T?i nhung d� h?t h?n ? x�a lu�n d? tr�nh t�ch luy b? nh?

        if (entry is not null)

        {
            _store.TryRemove(key, out _);

        }

        entry = null;

        return false;

    }

    /// <inheritdoc/>

    public void Set(CustomerCacheKey key, List<CustomerDto> customers, int totalCount)

    {
        _store[key] = new CustomerCacheEntry

        {
            Customers = customers,

            TotalCount = totalCount,

            ExpiresAt = DateTime.UtcNow.Add(Ttl)

        };

    }

    /// <inheritdoc/>

    public void Invalidate() => _store.Clear();
}