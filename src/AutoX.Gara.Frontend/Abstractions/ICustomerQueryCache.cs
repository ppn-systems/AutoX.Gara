// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Customers;
using AutoX.Gara.Shared.Protocol.Customers;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Cache danh sách khách hàng phía client (TTL = 30 giây).
/// Tránh g?i request l?p l?i khi user navigate qua l?i gi?a trang/tab.
/// </summary>
public interface ICustomerQueryCache
{
    /// <summary>
    /// Th? l?y k?t qu? dã cache.
    /// Tr? vụ <c>false</c> n?u không tìm th?y ho?c entry dã h?t h?n.
    /// </summary>
    System.Boolean TryGet(CustomerCacheKey key, out CustomerCacheEntry? entry);

    /// <summary>
    /// Luu k?t qu? mới vào cache.
    /// </summary>
    void Set(CustomerCacheKey key, System.Collections.Generic.List<CustomerDto> customers, System.Int32 totalCount);

    /// <summary>
    /// Xóa toàn b? cache.
    /// G?i sau mới Create / Update / Delete thành công d? d?m b?o
    /// l?n load ti?p theo l?y d? li?u mới nh?t t? server.
    /// </summary>
    void Invalidate();
}
