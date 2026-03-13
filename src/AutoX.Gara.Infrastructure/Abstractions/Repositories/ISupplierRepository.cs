// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Infrastructure.Abstractions.Repositories;

/// <summary>
/// Định nghĩa contract cho tất cả thao tác dữ liệu liên quan đến <see cref="Supplier"/>.
/// <para>
/// Application layer chỉ phụ thuộc vào interface này — không biết gì về EF Core.
/// </para>
/// </summary>
public interface ISupplierRepository
{
    // ─── Query ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách nhà cung cấp có phân trang, tìm kiếm, lọc và sắp xếp.
    /// </summary>
    System.Threading.Tasks.Task<(System.Collections.Generic.List<Supplier> Items, System.Int32 TotalCount)> GetPageAsync(
        SupplierListQuery query,
        System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Lấy chi tiết một nhà cung cấp theo ID,
    /// bao gồm navigation property <c>PhoneNumbers</c>.
    /// </summary>
    System.Threading.Tasks.Task<Supplier> GetByIdAsync(
        System.Int32 id,
        System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra nhà cung cấp đã tồn tại theo email hoặc mã số thuế.
    /// Dùng để tránh tạo trùng khi Create.
    /// </summary>
    System.Threading.Tasks.Task<System.Boolean> ExistsByContactAsync(
        System.String email,
        System.String taxCode,
        System.Threading.CancellationToken ct = default);

    // ─── Write ────────────────────────────────────────────────────────────────

    /// <summary>Thêm mới một nhà cung cấp (chưa SaveChanges).</summary>
    System.Threading.Tasks.Task AddAsync(
        Supplier supplier,
        System.Threading.CancellationToken ct = default);

    /// <summary>Đánh dấu entity là Modified (chưa SaveChanges).</summary>
    void Update(Supplier supplier);

    /// <summary>Lưu tất cả thay đổi vào database.</summary>
    System.Threading.Tasks.Task SaveChangesAsync(
        System.Threading.CancellationToken ct = default);
}