// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Shared.Enums;

namespace AutoX.Gara.Shared.Models;

/// <summary>
/// Value object đóng gói các tham số truy vấn danh sách nhân viên.
/// </summary>
public sealed record EmployeeListQuery(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    EmployeeSortField SortBy,
    System.Boolean SortDescending,
    Position FilterPosition,
    EmploymentStatus FilterStatus,
    Gender FilterGender);