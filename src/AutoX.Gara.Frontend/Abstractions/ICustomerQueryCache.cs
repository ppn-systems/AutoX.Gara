// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Frontend.Services.Customers;
using AutoX.Gara.Shared.Protocol.Customers;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Cache danh sách khách hàng phía client (TTL = 30 giây).
/// Tránh gửi request lặp lại khi user navigate qua lại giữa trang/tab.
/// </summary>
public interface ICustomerQueryCache
{
    /// <summary>
    /// Thử lấy kết quả đã cache.
    /// Trả về <c>false</c> nếu không tìm thấy hoặc entry đã hết hạn.
    /// </summary>
    System.Boolean TryGet(CustomerCacheKey key, out CustomerCacheEntry? entry);

    /// <summary>
    /// Lưu kết quả mới vào cache.
    /// </summary>
    void Set(CustomerCacheKey key, System.Collections.Generic.List<CustomerDto> customers, System.Int32 totalCount);

    /// <summary>
    /// Xóa toàn bộ cache.
    /// Gọi sau mỗi Create / Update / Delete thành công để đảm bảo
    /// lần load tiếp theo lấy dữ liệu mới nhất từ server.
    /// </summary>
    void Invalidate();
}