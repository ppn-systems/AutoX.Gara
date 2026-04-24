// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Frontend.Services.Customers;
using AutoX.Gara.Contracts.Customers;
using System.Collections.Generic;
namespace AutoX.Gara.Frontend.Abstractions;
/// <summary>
/// Cache danh s�ch kh�ch h�ng ph�a client (TTL = 30 gi�y).
/// Tr�nh g?i request l?p l?i khi user navigate qua l?i gi?a trang/tab.
/// </summary>
public interface ICustomerQueryCache
{
    /// <summary>
    /// Th? l?y k?t qu? d� cache.
    /// Tr? v? <c>false</c> n?u kh�ng t�m tH?y ho?c entry d� h?t h?n.
    /// </summary>
    bool TryGet(CustomerCacheKey key, out CustomerCacheEntry? entry);
    /// <summary>
    /// Luu k?t qu? m?i v�o cache.
    /// </summary>
    void Set(CustomerCacheKey key, List<CustomerDto> customers, int totalCount);
    /// <summary>
    /// X�a to�n b? cache.
    /// G?i sau m?i Create / Update / Delete th�nh c�ng d? d?m b?o
    /// l?n load ti?p theo l?y dữ liệu m?i nh?t t? server.
    /// </summary>
    void Invalidate();
}


