// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Frontend.Abstractions;
using AutoX.Gara.Shared.Protocol.Inventory;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Inventory;

/// <summary>
/// Key duy nhất cho một tập tham số truy vấn ReplacementPart.
/// </summary>
public sealed record ReplacementPartCacheKey(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    ReplacementPartSortField SortBy,
    System.Boolean SortDescending,
    System.Boolean? FilterInStock,
    System.Boolean? FilterDefective,
    System.Boolean? FilterExpired);

/// <summary>Một entry trong cache gồm dữ liệu và thời điểm hết hạn.</summary>
public sealed class ReplacementPartCacheEntry
{
    public required List<ReplacementPartDto> Parts { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}

/// <summary>
/// In-memory cache thread-safe với TTL 30 giây cho <c>ReplacementPart</c>.
/// Xóa toàn bộ cache khi có write operation để tránh stale data.
/// </summary>
public sealed class ReplacementPartQueryCache : IReplacementPartQueryCache
{
    private static readonly System.TimeSpan Ttl = System.TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<ReplacementPartCacheKey, ReplacementPartCacheEntry> _store = new();

    /// <inheritdoc/>
    public System.Boolean TryGet(ReplacementPartCacheKey key, out ReplacementPartCacheEntry? entry)
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

    /// <inheritdoc/>
    public void Set(ReplacementPartCacheKey key, List<ReplacementPartDto> parts, System.Int32 totalCount)
    {
        _store[key] = new ReplacementPartCacheEntry
        {
            Parts = parts,
            TotalCount = totalCount,
            ExpiresAt = System.DateTime.UtcNow.Add(Ttl)
        };
    }

    /// <inheritdoc/>
    public void Invalidate() => _store.Clear();
}
