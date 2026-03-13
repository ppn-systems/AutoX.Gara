// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Shared.Enums;

namespace AutoX.Gara.Shared.Models;

public sealed record ServiceItemListQuery(
    System.Int32 Page,
    System.Int32 PageSize,
    System.String SearchTerm,
    ServiceItemSortField SortBy,
    System.Boolean SortDescending,
    ServiceType? FilterType,
    System.Decimal? FilterMinUnitPrice,
    System.Decimal? FilterMaxUnitPrice)
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

        if (FilterMinUnitPrice.HasValue && FilterMaxUnitPrice.HasValue && FilterMinUnitPrice.Value > FilterMaxUnitPrice.Value)
        {
            throw new System.ArgumentException("FilterMinUnitPrice must be less than or equal to FilterMaxUnitPrice.");
        }
    }
}

