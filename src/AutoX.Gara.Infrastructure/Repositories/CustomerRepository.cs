// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Domain.Models;
using AutoX.Gara.Infrastructure.Abstractions;
using AutoX.Gara.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation của <see cref="ICustomerRepository"/>.
/// <para>
/// Infrastructure layer — đây là nơi DUY NHẤT được phép import
/// <c>Microsoft.EntityFrameworkCore</c> cho Customer queries.
/// </para>
/// </summary>
public sealed class CustomerRepository(AutoXDbContext context) : ICustomerRepository
{
    private readonly AutoXDbContext _context = context
        ?? throw new System.ArgumentNullException(nameof(context));

    // ─── Query ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "<Pending>")]
    public async Task<(System.Collections.Generic.List<Customer> Items, System.Int32 TotalCount)> GetPageAsync(
        CustomerListQuery query,
        CancellationToken ct = default)
    {
        IQueryable<Customer> q = _context.Customers
            .AsNoTracking()
            .Where(c => c.DeletedAt == null);

        // ── Search ─────────────────────────────────────────────────────────
        if (!System.String.IsNullOrWhiteSpace(query.SearchTerm))
        {
            System.String term = query.SearchTerm.Trim().ToLowerInvariant();
            q = q.Where(c =>
                (c.Name != null && c.Name.ToLower().Contains(term)) ||
                (c.Email != null && c.Email.ToLower().Contains(term)) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(term)) ||
                (c.Notes != null && c.Notes.ToLower().Contains(term)));
        }

        // ── Filter ─────────────────────────────────────────────────────────
        if (query.FilterType != CustomerType.None)
        {
            q = q.Where(c => c.Type == query.FilterType);
        }

        if (query.FilterMembership != MembershipLevel.None)
        {
            q = q.Where(c => c.Membership == query.FilterMembership);
        }

        // ── Sort ───────────────────────────────────────────────────────────
        q = (query.SortBy, query.SortDescending) switch
        {
            (CustomerSortField.Name, false) => q.OrderBy(c => c.Name),
            (CustomerSortField.Name, true) => q.OrderByDescending(c => c.Name),
            (CustomerSortField.Email, false) => q.OrderBy(c => c.Email),
            (CustomerSortField.Email, true) => q.OrderByDescending(c => c.Email),
            (CustomerSortField.CreatedAt, false) => q.OrderBy(c => c.CreatedAt),
            (CustomerSortField.CreatedAt, true) => q.OrderByDescending(c => c.CreatedAt),
            (CustomerSortField.UpdatedAt, false) => q.OrderBy(c => c.UpdatedAt),
            (CustomerSortField.UpdatedAt, true) => q.OrderByDescending(c => c.UpdatedAt),
            _ => q.OrderByDescending(c => c.CreatedAt)
        };

        // ── Count + Page ───────────────────────────────────────────────────
        System.Int32 totalCount = await q.CountAsync(ct).ConfigureAwait(false);

        System.Collections.Generic.List<Customer> items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public Task<Customer> GetByIdAsync(System.Int32 id, CancellationToken ct = default)
        => _context.Customers
                   .AsNoTracking()
                   .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);

    /// <inheritdoc/>
    public Task<System.Boolean> ExistsByContactAsync(
        System.String email,
        System.String phoneNumber,
        CancellationToken ct = default)
        => _context.Customers.AnyAsync(
            c => c.DeletedAt == null &&
                 (c.Email == email || c.PhoneNumber == phoneNumber),
            ct);

    // ─── Write ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public Task AddAsync(Customer customer, CancellationToken ct = default)
        => _context.Customers.AddAsync(customer, ct).AsTask();

    /// <inheritdoc/>
    public void Update(Customer customer)
        => _context.Customers.Update(customer);

    /// <inheritdoc/>
    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}