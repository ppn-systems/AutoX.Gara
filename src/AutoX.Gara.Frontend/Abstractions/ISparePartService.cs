// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Protocol.Inventory;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Service interface for working with spare parts (CRUD, cache).
/// </summary>
public interface ISparePartService
{
    /// <summary>
    /// Get a paged list of spare parts from cache/server.
    /// </summary>
    /// <param name="page">Page index (0-based).</param>
    /// <param name="pageSize">Size of each page.</param>
    /// <param name="searchTerm">Search term filter.</param>
    /// <param name="sortBy">Sort field.</param>
    /// <param name="sortDescending">Descending order?</param>
    /// <param name="filterSupplierId">Supplier filter (nullable).</param>
    /// <param name="filterCategory">Category filter (nullable).</param>
    /// <param name="filterDiscontinued">Discontinued filter (nullable).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result including entries and metadata.</returns>
    Task<SparePartListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        SparePartSortField sortBy = SparePartSortField.PartName,
        System.Boolean sortDescending = false,
        System.Int32? filterSupplierId = null,
        PartCategory? filterCategory = null,
        System.Boolean? filterDiscontinued = null,
        CancellationToken ct = default);

    /// <summary>
    /// Create a new spare part.
    /// </summary>
    /// <param name="data">Part data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Write result.</returns>
    Task<SparePartWriteResult> CreateAsync(
        SparePartDto data,
        CancellationToken ct = default);

    /// <summary>
    /// Update an existing spare part.
    /// </summary>
    /// <param name="data">Part data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Write result.</returns>
    Task<SparePartWriteResult> UpdateAsync(
        SparePartDto data,
        CancellationToken ct = default);

    /// <summary>
    /// Discontinue (delete) a spare part entry.
    /// </summary>
    /// <param name="data">Part data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Write result.</returns>
    Task<SparePartWriteResult> DiscontinueAsync(
        SparePartDto data,
        CancellationToken ct = default);
}