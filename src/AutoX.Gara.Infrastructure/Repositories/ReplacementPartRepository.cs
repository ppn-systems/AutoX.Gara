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
/// EF Core implementation của <see cref="IReplacementPartRepository"/>.
/// <para>
/// Infrastructure layer — đây là nơi DUY NHẤT được phép import
/// <c>Microsoft.EntityFrameworkCore</c> cho ReplacementPart queries.
/// </para>
/// </summary>
public sealed class ReplacementPartRepository(AutoXDbContext context) : IReplacementPartRepository
{
    private readonly AutoXDbContext _context = context
        ?? throw new System.ArgumentNullException(nameof(context));

    // ─── Query ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EF Core translates ToLower() to SQL LOWER() — StringComparison overloads are not supported by the provider.")]
    public async System.Threading.Tasks.Task<(System.Collections.Generic.List<ReplacementPart> Items, System.Int32 TotalCount)> GetPageAsync(
        ReplacementPartListQuery query,
        System.Threading.CancellationToken ct = default)
    {
        IQueryable<ReplacementPart> q = _context.ReplacementParts.AsNoTracking();

        // ── Search ─────────────────────────────────────────────────────────
        if (!System.String.IsNullOrWhiteSpace(query.SearchTerm))
        {
            System.String term = query.SearchTerm.Trim().ToLowerInvariant();
            q = q.Where(r =>
                (r.PartName != null && r.PartName.ToLower().Contains(term)) ||
                (r.PartCode != null && r.PartCode.ToLower().Contains(term)) ||
                (r.Manufacturer != null && r.Manufacturer.ToLower().Contains(term)));
        }

        // ── Filter: InStock ────────────────────────────────────────────────
        if (query.FilterInStock.HasValue)
        {
            q = query.FilterInStock.Value
                ? q.Where(r => r.Quantity > 0)
                : q.Where(r => r.Quantity == 0);
        }

        // ── Filter: Defective ──────────────────────────────────────────────
        if (query.FilterDefective.HasValue)
        {
            q = q.Where(r => r.IsDefective == query.FilterDefective.Value);
        }

        // ── Filter: Expired ────────────────────────────────────────────────
        // So sánh với ngày hôm nay (DateOnly) — EF Core 8+ hỗ trợ DateOnly trong SQL
        if (query.FilterExpired.HasValue)
        {
            System.DateOnly today = System.DateOnly.FromDateTime(System.DateTime.UtcNow);
            q = query.FilterExpired.Value
                ? q.Where(r => r.ExpiryDate.HasValue && r.ExpiryDate.Value < today)
                : q.Where(r => !r.ExpiryDate.HasValue || r.ExpiryDate.Value >= today);
        }

        // ── Sort ───────────────────────────────────────────────────────────
        q = (query.SortBy, query.SortDescending) switch
        {
            (ReplacementPartSortField.PartName, false) => q.OrderBy(r => r.PartName),
            (ReplacementPartSortField.PartName, true) => q.OrderByDescending(r => r.PartName),
            (ReplacementPartSortField.DateAdded, false) => q.OrderBy(r => r.DateAdded),
            (ReplacementPartSortField.DateAdded, true) => q.OrderByDescending(r => r.DateAdded),
            (ReplacementPartSortField.ExpiryDate, false) => q.OrderBy(r => r.ExpiryDate),
            (ReplacementPartSortField.ExpiryDate, true) => q.OrderByDescending(r => r.ExpiryDate),
            (ReplacementPartSortField.Quantity, false) => q.OrderBy(r => r.Quantity),
            (ReplacementPartSortField.Quantity, true) => q.OrderByDescending(r => r.Quantity),
            (ReplacementPartSortField.UnitPrice, false) => q.OrderBy(r => r.UnitPrice),
            (ReplacementPartSortField.UnitPrice, true) => q.OrderByDescending(r => r.UnitPrice),
            _ => q.OrderByDescending(r => r.DateAdded)
        };

        // ── Count + Page ───────────────────────────────────────────────────
        System.Int32 totalCount = await q.CountAsync(ct).ConfigureAwait(false);

        System.Collections.Generic.List<ReplacementPart> items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public System.Threading.Tasks.Task<ReplacementPart> GetByIdAsync(
        System.Int32 id,
        System.Threading.CancellationToken ct = default)
        => _context.ReplacementParts
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    /// <inheritdoc/>
    public System.Threading.Tasks.Task<System.Boolean> ExistsByPartCodeAsync(
        System.String partCode,
        System.Threading.CancellationToken ct = default)
        => _context.ReplacementParts.AnyAsync(r => r.PartCode == partCode, ct);

    // ─── Write ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public System.Threading.Tasks.Task AddAsync(
        ReplacementPart part,
        System.Threading.CancellationToken ct = default)
        => _context.ReplacementParts.AddAsync(part, ct).AsTask();

    /// <inheritdoc/>
    public void Update(ReplacementPart part)
        => _context.ReplacementParts.Update(part);

    /// <inheritdoc/>
    public void Delete(ReplacementPart part)
        => _context.ReplacementParts.Remove(part);

    /// <inheritdoc/>
    public System.Threading.Tasks.Task SaveChangesAsync(
        System.Threading.CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
