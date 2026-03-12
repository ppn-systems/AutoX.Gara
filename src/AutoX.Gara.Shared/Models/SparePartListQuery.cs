// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;

namespace AutoX.Gara.Shared.Models;

/// <summary>
/// Value object chứa tham số truy vấn phân trang cho <c>SparePart</c>.
/// Được tạo từ <c>SparePartQueryRequest</c> packet tại Application layer,
/// sau đó truyền xuống Infrastructure layer — tách biệt hoàn toàn khỏi packet/EF.
/// </summary>
public sealed record SparePartListQuery(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    SparePartSortField SortBy,
    System.Boolean SortDescending,
    System.Int32? FilterSupplierId,
    PartCategory? FilterCategory,
    System.Boolean? FilterDiscontinued);
