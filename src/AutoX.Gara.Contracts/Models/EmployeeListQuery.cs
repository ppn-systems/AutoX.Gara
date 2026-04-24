// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Contracts.Enums;
namespace AutoX.Gara.Contracts.Models;
/// <summary>
/// Value object d�ng g�i c�c tham s? truy v?n danh s�ch nh�n vi�n.
/// </summary>
public sealed record EmployeeListQuery(
    int Page,
    int PageSize,
    string SearchTerm,
    EmployeeSortField SortBy,
    bool SortDescending,
    Position FilterPosition,
    EmploymentStatus FilterStatus,
    Gender FilterGender);

