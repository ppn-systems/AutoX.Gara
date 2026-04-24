// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Shared.Protocol.Vehicles;
using System;
using System.Collections.Generic;
namespace AutoX.Gara.Frontend.Services.Vehicles;
// --- Cache Entry -------------------------------------------------------------
/// <summary>
/// M?t entry trong cache xe g?m dữ liệu v� th?i di?m h?t h?n.
/// </summary>
public sealed class VehicleCacheEntry
{
    public required List<VehicleDto> Vehicles { get; init; }
    public required int TotalCount { get; init; }
    public required DateTime ExpiresAt { get; init; }
    /// <summary>true khi entry d� qu� TTL v� kh�ng c�n h?p l?.</summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
// --- Cache Key ----------------------------------------------------------------
/// <summary>
/// Key duy nh?t cho m?t t?p tham s? truy v?n danh s�ch xe.
/// Record t? sinh Equals + GetHashCode � d�ng được v?i ConcurrentDictionary.
/// </summary>
public sealed record VehicleCacheKey(
    int CustomerId,
    int Page,
    int PageSize);
// --- Cache --------------------------------------------------------------------
/// <summary>
/// In-memory cache thread-safe v?i TTL 30 gi�y cho danh s�ch xe.
/// Write operations ph?i g?i <see cref="Invalidate(int)"/> d? x�a cache c?a customer d�.
/// </summary>
public sealed class VehicleQueryCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);
    private readonly System.Collections.Concurrent.ConcurrentDictionary<VehicleCacheKey, VehicleCacheEntry> _store = new();
    public bool TryGet(VehicleCacheKey key, out VehicleCacheEntry? entry)
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
    public void Set(VehicleCacheKey key, List<VehicleDto> vehicles, int totalCount)
    {
        _store[key] = new VehicleCacheEntry
        {
            Vehicles = vehicles,
            TotalCount = totalCount,
            ExpiresAt = DateTime.UtcNow.Add(Ttl)
        };
    }
    /// <summary>X�a cache c?a m?t customer c? th? (sau write operation).</summary>
    public void Invalidate(int customerId)
    {
        foreach (VehicleCacheKey key in _store.Keys)
        {
            if (key.CustomerId == customerId)
            {
                _store.TryRemove(key, out _);
            }
        }
    }
    /// <summary>X�a to�n b? cache (d�ng khi kh�ng bi?t customerId).</summary>
    public void InvalidateAll() => _store.Clear();
}
