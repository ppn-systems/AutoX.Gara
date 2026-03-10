// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Packets.Customers;

namespace AutoX.Gara.Shared.Enums;

/// <summary>
/// Các cột được phép sắp xếp trong truy vấn danh sách khách hàng.
/// Dùng trong <see cref="CustomersQueryPacket.SortBy"/>.
/// </summary>
public enum CustomerSortField : System.Byte
{
    /// <summary>Sắp xếp theo ngày tạo (mặc định).</summary>
    CreatedAt = 0,

    /// <summary>Sắp xếp theo tên khách hàng (A–Z hoặc Z–A).</summary>
    Name = 1,

    /// <summary>Sắp xếp theo địa chỉ email.</summary>
    Email = 2,

    /// <summary>Sắp xếp theo ngày cập nhật gần nhất.</summary>
    UpdatedAt = 3,
}