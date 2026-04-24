// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Contracts.Enums;
using System;
namespace AutoX.Gara.Contracts.Models;
/// <summary>
/// Value object đóng gói các tham số truy vấn danh sách nhà cung cấp.
/// <para>
/// Được tạo từ <c>SupplierQueryRequest</c> packet ở Application layer,
/// truyền xuống Repository để tách biệt domain logic khỏi network protocol.
/// </para>
/// </summary>
/// <param name="Page">Số trang (bắt đầu từ 1).</param>
/// <param name="PageSize">Số bản ghi tối đa mỗi trang.</param>
/// <param name="SearchTerm">Từ khóa tìm theo tên, email. Rỗng = không filter.</param>
/// <param name="SortBy">Cột dùng để sắp xếp.</param>
/// <param name="SortDescending"><c>true</c> = giảm dần, <c>false</c> = tăng dần.</param>
/// <param name="FilterStatus">Lọc theo trạng thái. <see cref="SupplierStatus.None"/> = tất cả.</param>
/// <param name="FilterPaymentTerms">Lọc theo điều khoản thanh toán. <see cref="PaymentTerms.None"/> = tất cả.</param>
public sealed record SupplierListQuery(
    int Page,
    int PageSize,
    string SearchTerm,
    SupplierSortField SortBy,
    bool SortDescending,
    SupplierStatus FilterStatus,
    PaymentTerms FilterPaymentTerms)
{
    public void Validate()
    {
        if (Page < 1)
        {
            throw new ArgumentException("Page must be >= 1");
        }
        if (PageSize < 1)
        {
            throw new ArgumentException("PageSize must be >= 1");
        }
    }
}

