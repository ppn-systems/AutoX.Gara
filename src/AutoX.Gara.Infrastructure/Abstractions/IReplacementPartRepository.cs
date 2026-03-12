// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Infrastructure.Abstractions;

/// <summary>
/// Định nghĩa các thao tác truy vấn và ghi dữ liệu cho <see cref="ReplacementPart"/>.
/// </summary>
public interface IReplacementPartRepository
{
    // ─── Query ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách phụ tùng kho theo trang, có hỗ trợ tìm kiếm / lọc / sắp xếp.
    /// </summary>
    System.Threading.Tasks.Task<(System.Collections.Generic.List<ReplacementPart> Items, System.Int32 TotalCount)> GetPageAsync(
        ReplacementPartListQuery query,
        System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Lấy phụ tùng kho theo Id.
    /// </summary>
    System.Threading.Tasks.Task<ReplacementPart> GetByIdAsync(
        System.Int32 id,
        System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra phụ tùng kho có tồn tại theo PartCode không.
    /// Dùng để tránh tạo trùng mã SKU.
    /// </summary>
    System.Threading.Tasks.Task<System.Boolean> ExistsByPartCodeAsync(
        System.String partCode,
        System.Threading.CancellationToken ct = default);

    // ─── Write ────────────────────────────────────────────────────────────────

    /// <summary>Thêm mới một phụ tùng kho.</summary>
    System.Threading.Tasks.Task AddAsync(
        ReplacementPart part,
        System.Threading.CancellationToken ct = default);

    /// <summary>Cập nhật phụ tùng kho (đánh dấu Modified).</summary>
    void Update(ReplacementPart part);

    /// <summary>Xóa vĩnh viễn một phụ tùng kho.</summary>
    void Delete(ReplacementPart part);

    /// <summary>Lưu tất cả thay đổi vào database.</summary>
    System.Threading.Tasks.Task SaveChangesAsync(
        System.Threading.CancellationToken ct = default);
}
