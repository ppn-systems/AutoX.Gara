// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Contracts.Enums;
using System;
namespace AutoX.Gara.Contracts.Models;
public sealed record ServiceItemListQuery(
    int Page,
    int PageSize,
    string SearchTerm,
    ServiceItemSortField SortBy,
    bool SortDescending,
    ServiceType? FilterType,
    decimal? FilterMinUnitPrice,
    decimal? FilterMaxUnitPrice)
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
        if (FilterMinUnitPrice.HasValue && FilterMaxUnitPrice.HasValue && FilterMinUnitPrice.Value > FilterMaxUnitPrice.Value)
        {
            throw new ArgumentException("FilterMinUnitPrice must be less than or equal to FilterMaxUnitPrice.");
        }
    }
}

