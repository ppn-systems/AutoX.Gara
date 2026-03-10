// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Packets.Customers;
using System.Collections.Generic;

namespace AutoX.Gara.Frontend.Services.Customers;

/// <summary>
/// Một entry trong cache gồm dữ liệu và thời điểm hết hạn.
/// </summary>
public sealed class CustomerCacheEntry
{
    public required List<CustomerDataPacket> Customers { get; init; }
    public required System.Int32 TotalCount { get; init; }
    public required System.DateTime ExpiresAt { get; init; }

    /// <summary>
    /// <c>true</c> khi entry đã quá TTL và không còn hợp lệ.
    /// </summary>
    public System.Boolean IsExpired => System.DateTime.UtcNow >= ExpiresAt;
}