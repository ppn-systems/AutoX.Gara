// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Vehicles;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Vehicles;

// --- Cache Entry -------------------------------------------------------------

/// <summary>
/// M?t entry trong cache xe g?m d? li?u và th?i di?m h?t h?n.
/// </summary>
public sealed class VehicleCacheEntry
{
    public required List<VehicleDto> Vehicles { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }

    /// <summary>true khi entry dã quá TTL và không còn h?p l?.</summary>
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

// --- Cache Key ----------------------------------------------------------------

/// <summary>
/// Key duy nh?t cho m?t t?p tham s? truy vụn danh sách xe.
/// Record t? sinh Equals + GetHashCode — dùng du?c vụi ConcurrentDictionary.
/// </summary>
public sealed record VehicleCacheKey(
    System.Int32 CustomerId,
    System.Int32 Page,
    System.Int32 PageSize);

// --- Cache --------------------------------------------------------------------

/// <summary>
/// In-memory cache thread-safe vụi TTL 30 giây cho danh sách xe.
/// Write operations phụi g?i <see cref="Invalidate(System.Int32)"/> d? xóa cache c?a customer dó.
/// </summary>
public sealed class VehicleQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);

    private readonly System.Collections.Concurrent.ConcurrentDictionary<VehicleCacheKey, VehicleCacheEntry> _store = new();

    public System.Boolean TryGet(VehicleCacheKey key, out VehicleCacheEntry? entry)
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

    public void Set(VehicleCacheKey key, List<VehicleDto> vehicles, System.Int32 totalCount)
    {
        _store[key] = new VehicleCacheEntry
        {
            Vehicles = vehicles,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    /// <summary>Xóa cache c?a m?t customer c? th? (sau write operation).</summary>
    public void Invalidate(System.Int32 customerId)
    {
        foreach (VehicleCacheKey key in _store.Keys)
        {
            if (key.CustomerId == customerId)
            {
                _store.TryRemove(key, out _);
            }
        }
    }

    /// <summary>Xóa toàn b? cache (dùng khi không bi?t customerId).</summary>
    public void InvalidateAll() => _store.Clear();
}
