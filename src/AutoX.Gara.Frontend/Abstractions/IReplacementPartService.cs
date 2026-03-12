// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Frontend.ViewModels.Results;
using AutoX.Gara.Shared.Protocol.Inventory;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.Abstractions;

/// <summary>
/// Service interface for working with replacement parts (CRUD, cache).
/// </summary>
public interface IReplacementPartService
{
    /// <summary>
    /// Get a paged list of replacement parts from cache/server.
    /// </summary>
    /// <param name="page">Page index (0-based).</param>
    /// <param name="pageSize">Size of each page.</param>
    /// <param name="searchTerm">Search term filter.</param>
    /// <param name="sortBy">Sort field.</param>
    /// <param name="sortDescending">Descending order?</param>
    /// <param name="filterInStock">Filter by in-stock (nullable).</param>
    /// <param name="filterDefective">Filter by defective state (nullable).</param>
    /// <param name="filterExpired">Filter by expired state (nullable).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result including entries and metadata.</returns>
    Task<ReplacementPartListResult> GetListAsync(
        System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        ReplacementPartSortField sortBy = ReplacementPartSortField.DateAdded,
        System.Boolean sortDescending = true,
        System.Boolean? filterInStock = null,
        System.Boolean? filterDefective = null,
        System.Boolean? filterExpired = null,
        CancellationToken ct = default);

    /// <summary>
    /// Create a new replacement part.
    /// </summary>
    /// <param name="data">Part data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Write result.</returns>
    Task<ReplacementPartWriteResult> CreateAsync(
        ReplacementPartDto data,
        CancellationToken ct = default);

    /// <summary>
    /// Update an existing replacement part.
    /// </summary>
    /// <param name="data">Part data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Write result.</returns>
    Task<ReplacementPartWriteResult> UpdateAsync(
        ReplacementPartDto data,
        CancellationToken ct = default);

    /// <summary>
    /// Delete a replacement part.
    /// </summary>
    /// <param name="data">Part data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Write result.</returns>
    Task<ReplacementPartWriteResult> DeleteAsync(
        ReplacementPartDto data,
        CancellationToken ct = default);
}