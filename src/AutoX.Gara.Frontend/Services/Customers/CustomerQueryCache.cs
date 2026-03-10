// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Shared.Packets.Customers;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Customers;

/// <summary>
/// In-memory cache thread-safe với TTL 30 giây.
/// <para>
/// Vòng đời cache: mỗi (page, pageSize, search, filter, sort) là một entry độc lập.
/// Khi user thực hiện write operation, toàn bộ cache bị xóa để tránh stale data.
/// </para>
/// </summary>
public sealed class CustomerQueryCache : ICustomerQueryCache
{
    /// <summary>TTL 30 giây — đủ để tránh duplicate request khi navigate, đủ ngắn để data không stale.</summary>
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<CustomerCacheKey, CustomerCacheEntry> _store = new();

    /// <inheritdoc/>
    public System.Boolean TryGet(CustomerCacheKey key, out CustomerCacheEntry? entry)
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
    public void Set(CustomerCacheKey key, List<CustomerDataPacket> customers, System.Int32 totalCount)
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