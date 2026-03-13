// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Shared.Enums;

namespace AutoX.Gara.Shared.Models;

/// <summary>
/// Value object representing query parameters for retrieving a paginated list of parts.
/// </summary>
public sealed record PartListQuery(
    /// <summary>Page number (starts from 1).</summary>
    System.Int32 Page,

    /// <summary>Number of records per page.</summary>
    System.Int32 PageSize,

    /// <summary>Search term for filtering parts by name, code, or manufacturer.</summary>
    System.String SearchTerm,

    /// <summary>Field to sort by.</summary>
    PartSortField SortBy,

    /// <summary>Sort order: true for descending, false for ascending.</summary>
    System.Boolean SortDescending,

    /// <summary>Filter by supplier identifier. Null means no filter.</summary>
    System.Int32? FilterSupplierId,

    /// <summary>Filter by part category. Null means no filter.</summary>
    PartCategory? FilterCategory,

    /// <summary>Filter by in-stock status. Null means no filter.</summary>
    System.Boolean? FilterInStock,

    /// <summary>Filter by defective status. Null means no filter.</summary>
    System.Boolean? FilterDefective,

    /// <summary>Filter by expired status. Null means no filter.</summary>
    System.Boolean? FilterExpired,

    /// <summary>Filter by discontinued status. Null means no filter.</summary>
    System.Boolean? FilterDiscontinued
)
{
    /// <summary>
    /// Validates the query parameters.
    /// </summary>
    /// <exception cref="System.ArgumentException">Thrown when validation fails.</exception>
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