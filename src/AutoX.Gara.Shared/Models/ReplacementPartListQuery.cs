// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;

namespace AutoX.Gara.Shared.Models;

/// <summary>
/// Value object chứa tham số truy vấn phân trang cho <c>ReplacementPart</c>.
/// </summary>
public sealed record ReplacementPartListQuery(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    ReplacementPartSortField SortBy,
    System.Boolean SortDescending,
    System.Boolean? FilterInStock,
    System.Boolean? FilterDefective,
    System.Boolean? FilterExpired);