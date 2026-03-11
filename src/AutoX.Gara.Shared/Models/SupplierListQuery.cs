// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Shared.Enums;

namespace AutoX.Gara.Shared.Models;

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
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    SupplierSortField SortBy,
    System.Boolean SortDescending,
    SupplierStatus FilterStatus,
    PaymentTerms FilterPaymentTerms);