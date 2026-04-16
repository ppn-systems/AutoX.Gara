using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Shared.Models;

public sealed record RepairOrderItemListQuery(
    int Page,
    int PageSize,
    string SearchTerm,
    RepairOrderItemSortField SortBy,
    bool SortDescending,
    int? FilterRepairOrderId,
    int? FilterPartId)
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
    }
}