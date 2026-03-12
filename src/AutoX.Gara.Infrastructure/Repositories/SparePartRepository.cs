// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Domain.Enums.Parts;
using AutoX.Gara.Infrastructure.Abstractions;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AutoX.Gara.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation của <see cref="ISparePartRepository"/>.
/// <para>
/// Infrastructure layer — đây là nơi DUY NHẤT được phép import
/// <c>Microsoft.EntityFrameworkCore</c> cho SparePart queries.
/// </para>
/// </summary>
public sealed class SparePartRepository(AutoXDbContext context) : ISparePartRepository
{
    private readonly AutoXDbContext _context = context
        ?? throw new System.ArgumentNullException(nameof(context));

    // ─── Query ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EF Core translates ToLower() to SQL LOWER() — StringComparison overloads are not supported by the provider.")]
    public async System.Threading.Tasks.Task<(System.Collections.Generic.List<SparePart> Items, System.Int32 TotalCount)> GetPageAsync(
        SparePartListQuery query,
        System.Threading.CancellationToken ct = default)
    {
        IQueryable<SparePart> q = _context.SpareParts
            .AsNoTracking()
            .Include(s => s.Supplier);

        // ── Search ─────────────────────────────────────────────────────────
        if (!System.String.IsNullOrWhiteSpace(query.SearchTerm))
        {
            System.String term = query.SearchTerm.Trim().ToLowerInvariant();
            q = q.Where(s => s.PartName != null && s.PartName.ToLower().Contains(term));
        }

        // ── Filter: Supplier ───────────────────────────────────────────────
        if (query.FilterSupplierId.HasValue)
        {
            q = q.Where(s => s.SupplierId == query.FilterSupplierId.Value);
        }

        // ── Filter: Category ───────────────────────────────────────────────
        if (query.FilterCategory.HasValue)
        {
            q = q.Where(s => s.PartCategory == query.FilterCategory.Value);
        }

        // ── Filter: Discontinued ───────────────────────────────────────────
        if (query.FilterDiscontinued.HasValue)
        {
            q = q.Where(s => s.IsDiscontinued == query.FilterDiscontinued.Value);
        }

        // ── Sort ───────────────────────────────────────────────────────────
        q = (query.SortBy, query.SortDescending) switch
        {
            (SparePartSortField.PartName, false) => q.OrderBy(s => s.PartName),
            (SparePartSortField.PartName, true) => q.OrderByDescending(s => s.PartName),
            (SparePartSortField.PurchasePrice, false) => q.OrderBy(s => s.PurchasePrice),
            (SparePartSortField.PurchasePrice, true) => q.OrderByDescending(s => s.PurchasePrice),
            (SparePartSortField.SellingPrice, false) => q.OrderBy(s => s.SellingPrice),
            (SparePartSortField.SellingPrice, true) => q.OrderByDescending(s => s.SellingPrice),
            (SparePartSortField.InventoryQuantity, false) => q.OrderBy(s => s.InventoryQuantity),
            (SparePartSortField.InventoryQuantity, true) => q.OrderByDescending(s => s.InventoryQuantity),
            _ => q.OrderBy(s => s.PartName)
        };

        // ── Count + Page ───────────────────────────────────────────────────
        System.Int32 totalCount = await q.CountAsync(ct).ConfigureAwait(false);

        System.Collections.Generic.List<SparePart> items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public System.Threading.Tasks.Task<SparePart> GetByIdAsync(
        System.Int32 id,
        System.Threading.CancellationToken ct = default)
        => _context.SpareParts
            .AsNoTracking()
            .Include(s => s.Supplier)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    /// <inheritdoc/>
    public System.Threading.Tasks.Task<System.Boolean> ExistsByNameAndSupplierAsync(
        System.String partName,
        System.Int32 supplierId,
        System.Threading.CancellationToken ct = default)
        => _context.SpareParts.AnyAsync(
            s => s.PartName == partName && s.SupplierId == supplierId, ct);

    // ─── Write ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public System.Threading.Tasks.Task AddAsync(
        SparePart sparePart,
        System.Threading.CancellationToken ct = default)
        => _context.SpareParts.AddAsync(sparePart, ct).AsTask();

    /// <inheritdoc/>
    public void Update(SparePart sparePart)
        => _context.SpareParts.Update(sparePart);

    /// <inheritdoc/>
    public System.Threading.Tasks.Task SaveChangesAsync(
        System.Threading.CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
