using AutoX.Gara.Shared.Enums;
using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using Nalix.Common.Networking.Protocols;

namespace AutoX.Gara.Shared.Models;

/// <summary>
/// Value object representing query parameters for retrieving a paginated list of parts.
/// </summary>
public sealed record PartListQuery(
    /// <summary>Page number (starts from 1).</summary>
    int Page,

    /// <summary>Number of records per page.</summary>
    int PageSize,

    /// <summary>Search term for filtering parts by name, code, or manufacturer.</summary>
    string SearchTerm,

    /// <summary>Field to sort by.</summary>
    PartSortField SortBy,

    /// <summary>Sort order: true for descending, false for ascending.</summary>
    bool SortDescending,

    /// <summary>Filter by supplier identifier. Null means no filter.</summary>
    int? FilterSupplierId,

    /// <summary>Filter by part category. Null means no filter.</summary>
    PartCategory? FilterCategory,

    /// <summary>Filter by in-stock status. Null means no filter.</summary>
    bool? FilterInStock,

    /// <summary>Filter by defective status. Null means no filter.</summary>
    bool? FilterDefective,

    /// <summary>Filter by expired status. Null means no filter.</summary>
    bool? FilterExpired,

    /// <summary>Filter by discontinued status. Null means no filter.</summary>
    bool? FilterDiscontinued
)
{
    /// <summary>
    /// Validates the query parameters.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
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