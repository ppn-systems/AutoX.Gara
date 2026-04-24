// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Contracts.Enums;
using System;
namespace AutoX.Gara.Contracts.Models;
public sealed record EmployeeSalaryListQuery(
    int Page,
    int PageSize,
    string SearchTerm,
    EmployeeSalarySortField SortBy,
    bool SortDescending,
    int? FilterEmployeeId,
    SalaryType? FilterSalaryType,
    DateTime? FilterFromDate,
    DateTime? FilterToDate)
{
    public void Validate()
    {
        if (Page < 1)
        {
            throw new ArgumentException("Page must be at least 1.");
        }
        if (PageSize < 1)
        {
            throw new ArgumentException("PageSize must be at least 1.");
        }
        if (FilterFromDate.HasValue && FilterToDate.HasValue && FilterFromDate.Value > FilterToDate.Value)
        {
            throw new ArgumentException("FilterFromDate must be <= FilterToDate.");
        }
    }
}

