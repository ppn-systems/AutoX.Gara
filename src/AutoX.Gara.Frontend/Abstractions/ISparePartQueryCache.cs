// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Inventory;
using AutoX.Gara.Shared.Protocol.Inventory;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Interface for SparePartQueryCache: in-memory cache with TTL for SparePart queries.
/// </summary>
public interface ISparePartQueryCache
{
    /// <summary>
    /// Try to get cached entry by key.
    /// </summary>
    /// <param name="key">Unique query key.</param>
    /// <param name="entry">Cached entry if present & not expired.</param>
    /// <returns>True if hit and not expired; else false.</returns>
    System.Boolean TryGet(SparePartCacheKey key, out SparePartCacheEntry? entry);

    /// <summary>
    /// Cache a result.
    /// </summary>
    /// <param name="key">Unique query key.</param>
    /// <param name="parts">List of spare parts.</param>
    /// <param name="totalCount">Total count (for pagination, etc).</param>
    void Set(SparePartCacheKey key, List<SparePartDto> parts, System.Int32 totalCount);

    /// <summary>
    /// Invalidate all cache entries.
    /// </summary>
    void Invalidate();
}