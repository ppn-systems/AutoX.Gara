// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Protocol.Inventory;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Interface for a cached replacement part entry.
/// </summary>
public interface IReplacementPartCacheEntry
{
    /// <summary>
    /// List of replacement parts.
    /// </summary>
    List<ReplacementPartDto> Parts { get; }

    /// <summary>
    /// Total count (useful for paging, etc).
    /// </summary>
    System.Int32 TotalCount { get; }

    /// <summary>
    /// Expiration time (UTC).
    /// </summary>
    System.DateTime ExpiresAt { get; }

    /// <summary>
    /// Indicates if the entry is expired.
    /// </summary>
    System.Boolean IsExpired { get; }
}