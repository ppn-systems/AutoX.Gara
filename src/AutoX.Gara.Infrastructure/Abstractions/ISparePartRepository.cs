// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Infrastructure.Abstractions;

/// <summary>
/// Định nghĩa các thao tác truy vấn và ghi dữ liệu cho <see cref="SparePart"/>.
/// </summary>
public interface ISparePartRepository
{
    // ─── Query ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách phụ tùng theo trang, có hỗ trợ tìm kiếm / lọc / sắp xếp.
    /// </summary>
    System.Threading.Tasks.Task<(System.Collections.Generic.List<SparePart> Items, System.Int32 TotalCount)> GetPageAsync(
        SparePartListQuery query,
        System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Lấy phụ tùng theo Id (chỉ trả về nếu chưa bị discontinued).
    /// </summary>
    System.Threading.Tasks.Task<SparePart> GetByIdAsync(
        System.Int32 id,
        System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra phụ tùng có tồn tại theo tên và SupplierId không.
    /// Dùng để tránh tạo trùng.
    /// </summary>
    System.Threading.Tasks.Task<System.Boolean> ExistsByNameAndSupplierAsync(
        System.String partName,
        System.Int32 supplierId,
        System.Threading.CancellationToken ct = default);

    // ─── Write ────────────────────────────────────────────────────────────────

    /// <summary>Thêm mới một phụ tùng.</summary>
    System.Threading.Tasks.Task AddAsync(
        SparePart sparePart,
        System.Threading.CancellationToken ct = default);

    /// <summary>Cập nhật phụ tùng (đánh dấu Modified).</summary>
    void Update(SparePart sparePart);

    /// <summary>Lưu tất cả thay đổi vào database.</summary>
    System.Threading.Tasks.Task SaveChangesAsync(
        System.Threading.CancellationToken ct = default);
}
