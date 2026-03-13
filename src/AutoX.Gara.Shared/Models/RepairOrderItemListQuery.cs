// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Shared.Enums;

namespace AutoX.Gara.Shared.Models;

public sealed record RepairOrderItemListQuery(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    RepairOrderItemSortField SortBy,
    System.Boolean SortDescending,
    System.Int32? FilterRepairOrderId,
    System.Int32? FilterPartId)
{
    public void Validate()
    {
        if (Page < 1)
        {
            throw new System.ArgumentException("Page must be at least 1.");
        }

        if (PageSize < 1)
        {
            throw new System.ArgumentException("PageSize must be at least 1.");
        }
    }
}

