// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Customers;

namespace AutoX.Gara.Domain.Models;

/// <summary>
/// Value object chứa tất cả tham số truy vấn danh sách khách hàng.
/// Dùng C# record để tự sinh Equals/GetHashCode — dùng được làm cache key.
/// </summary>
public sealed record CustomerListQuery(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    CustomerSortField SortBy,
    System.Boolean SortDescending,
    CustomerType FilterType,
    MembershipLevel FilterMembership);