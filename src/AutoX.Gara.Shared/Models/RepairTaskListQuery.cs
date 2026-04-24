// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Repairs;
using AutoX.Gara.Shared.Enums;
using System;

namespace AutoX.Gara.Shared.Models;

public sealed record RepairTaskListQuery(
    int Page,
    int PageSize,
    string SearchTerm,
    RepairTaskSortField SortBy,
    bool SortDescending,
    int? FilterRepairOrderId,
    int? FilterEmployeeId,
    int? FilterServiceItemId,
    RepairOrderStatus? FilterStatus,
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
            throw new ArgumentException("FilterFromDate must be less than or equal to FilterToDate.");
        }
    }
}
