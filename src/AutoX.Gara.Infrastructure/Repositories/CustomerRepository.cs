using AutoX.Gara.Application.Repositories;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Domain.Enums.Customers;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
namespace AutoX.Gara.Infrastructure.Repositories;
/// <summary>
/// EF Core implementation c?a <see cref="ICustomerRepository"/>.
/// <para>
/// Infrastructure layer � d�y l� noi DUY NH?T được ph�p import
/// <c>Microsoft.EntityFrameworkCore</c> cho Customer queries.
/// </para>
/// </summary>
public sealed class CustomerRepository(AutoXDbContext context) : ICustomerRepository
{
    private readonly AutoXDbContext _context = context
        ?? throw new System.ArgumentNullException(nameof(context));
    // --- Query ----------------------------------------------------------------
    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "<Pending>")]
    public async System.Threading.Tasks.Task<(List<Customer> Items, int TotalCount)> GetPageAsync(
        CustomerListQuery query, System.Threading.CancellationToken ct = default)
    {
        IQueryable<Customer> q = _context.Customers.AsNoTracking().Where(c => c.DeletedAt == null);
        // -- Search ---------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            string term = query.SearchTerm.Trim().ToLowerInvariant();
            q = q.Where(c =>
                (c.Name != null && c.Name.ToLower().Contains(term)) ||
                (c.Email != null && c.Email.ToLower().Contains(term)) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(term)) ||
                (c.Notes != null && c.Notes.ToLower().Contains(term)));
        }
        // -- Filter ---------------------------------------------------------
        if (query.FilterType != CustomerType.None)
        {
            q = q.Where(c => c.Type == query.FilterType);
        }
        if (query.FilterMembership != MembershipLevel.None)
        {
            q = q.Where(c => c.Membership == query.FilterMembership);
        }
        // -- Sort -----------------------------------------------------------
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
        // -- Count + Page ---------------------------------------------------
        int totalCount = await q.CountAsync(ct).ConfigureAwait(false);
        List<Customer> items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        return (items, totalCount);
    }
    /// <inheritdoc/>
    public System.Threading.Tasks.Task<Customer> GetByIdAsync(int id, System.Threading.CancellationToken ct = default)
        => _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null, ct);
    /// <inheritdoc/>
    public System.Threading.Tasks.Task<bool> ExistsByContactAsync(string email, string phoneNumber, System.Threading.CancellationToken ct = default)
        => _context.Customers.AnyAsync(c => c.DeletedAt == null && (c.Email == email || c.PhoneNumber == phoneNumber), ct);
    // --- Write ----------------------------------------------------------------
    /// <inheritdoc/>
    public System.Threading.Tasks.Task AddAsync(Customer customer, System.Threading.CancellationToken ct = default) => _context.Customers.AddAsync(customer, ct).AsTask();
    /// <inheritdoc/>
    public void Update(Customer customer) => _context.Customers.Update(customer);
    public void Delete(Customer customer)
    {
        customer.DeletedAt = System.DateTime.UtcNow;
        _context.Customers.Update(customer);
    }
    /// <inheritdoc/>
    public System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}


