using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Employees;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Shared.Models;

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