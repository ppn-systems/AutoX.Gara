// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Vehicles;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Vehicles;

// ─── Cache Entry ─────────────────────────────────────────────────────────────

/// <summary>
/// Một entry trong cache xe gồm dữ liệu và thời điểm hết hạn.
/// </summary>
public sealed class VehicleCacheEntry
{
    public required List<VehicleDto> Vehicles { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }

    /// <summary>true khi entry đã quá TTL và không còn hợp lệ.</summary>
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

// ─── Cache Key ────────────────────────────────────────────────────────────────

/// <summary>
/// Key duy nhất cho một tập tham số truy vấn danh sách xe.
/// Record tự sinh Equals + GetHashCode — dùng được với ConcurrentDictionary.
/// </summary>
public sealed record VehicleCacheKey(
    System.Int32 CustomerId,
    System.Int32 Page,
    System.Int32 PageSize);

// ─── Cache ────────────────────────────────────────────────────────────────────

/// <summary>
/// In-memory cache thread-safe với TTL 30 giây cho danh sách xe.
/// Write operations phải gọi <see cref="Invalidate(System.Int32)"/> để xóa cache của customer đó.
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

    /// <summary>Xóa cache của một customer cụ thể (sau write operation).</summary>
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

    /// <summary>Xóa toàn bộ cache (dùng khi không biết customerId).</summary>
    public void InvalidateAll() => _store.Clear();
}