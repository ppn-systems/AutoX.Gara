using AutoX.Gara.Application.Abstractions.Repositories;
// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
namespace AutoX.Gara.Infrastructure.Repositories;
/// <summary>
/// EF Core implementation of ISupplierRepository.
/// </summary>
public sealed class SupplierRepository(AutoXDbContext context) : ISupplierRepository
{
    private readonly AutoXDbContext _context = context
        ?? throw new System.ArgumentNullException(nameof(context));
    // --- Query ---
    /// <inheritdoc/>
    public async System.Threading.Tasks.Task<(List<Supplier> Items, int TotalCount)> GetPageAsync(
        SupplierListQuery query,
        System.Threading.CancellationToken ct = default)
    {
        query.Validate();
        IQueryable<Supplier> q = _context.Suppliers
            .AsNoTracking()
            .Where(s => s.DeletedAt == null)
            .Include(s => s.PhoneNumbers);
        // -- Search --
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            string term = query.SearchTerm.Trim().ToLowerInvariant();
            q = q.Where(s =>
                (s.Name != null && s.Name.ToLower().Contains(term)) ||
                (s.Email != null && s.Email.ToLower().Contains(term)) ||
                (s.TaxCode != null && s.TaxCode.Contains(term)) ||
                (s.Notes != null && s.Notes.ToLower().Contains(term)));
        }
        // -- Filter --
        if (query.FilterStatus != SupplierStatus.None)
        {
            q = q.Where(s => s.Status == query.FilterStatus);
        }
        if (query.FilterPaymentTerms != PaymentTerms.None)
        {
            q = q.Where(s => s.PaymentTerms == query.FilterPaymentTerms);
        }
        // -- Sort --
        q = (query.SortBy, query.SortDescending) switch
        {
            (SupplierSortField.Name, false) => q.OrderBy(s => s.Name),
            (SupplierSortField.Name, true) => q.OrderByDescending(s => s.Name),
            (SupplierSortField.Email, false) => q.OrderBy(s => s.Email),
            (SupplierSortField.Email, true) => q.OrderByDescending(s => s.Email),
            (SupplierSortField.ContractStartDate, false) => q.OrderBy(s => s.ContractStartDate),
            (SupplierSortField.ContractStartDate, true) => q.OrderByDescending(s => s.ContractStartDate),
            (SupplierSortField.ContractEndDate, false) => q.OrderBy(s => s.ContractEndDate),
            (SupplierSortField.ContractEndDate, true) => q.OrderByDescending(s => s.ContractEndDate),
            (SupplierSortField.Status, false) => q.OrderBy(s => s.Status),
            (SupplierSortField.Status, true) => q.OrderByDescending(s => s.Status),
            _ => q.OrderBy(s => s.Name)
        };
        // -- Count + Page --
        int totalCount = await q.CountAsync(ct).ConfigureAwait(false);
        List<Supplier> items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        return (items, totalCount);
    }
    /// <inheritdoc/>
    public System.Threading.Tasks.Task<Supplier> GetByIdAsync(
        int id,
        System.Threading.CancellationToken ct = default)
        => _context.Suppliers
            .AsNoTracking()
            .Include(s => s.PhoneNumbers)
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, ct);
    /// <inheritdoc/>
    public System.Threading.Tasks.Task<bool> ExistsByDetailsAsync(
        string name,
        string email,
        string phoneNumber,
        System.Threading.CancellationToken ct = default)
        => _context.Suppliers.AnyAsync(
            s => s.DeletedAt == null
                && (s.Name == name || s.Email == email || s.PhoneNumber == phoneNumber),
            ct);
    // --- Write ---
    /// <inheritdoc/>
    public System.Threading.Tasks.Task AddAsync(
        Supplier supplier,
        System.Threading.CancellationToken ct = default)
        => _context.Suppliers.AddAsync(supplier, ct).AsTask();
    /// <inheritdoc/>
    public void Update(Supplier supplier)
        => _context.Suppliers.Update(supplier);
    public void Delete(Supplier supplier)
    {
        supplier.DeletedAt = System.DateTime.UtcNow;
        _context.Suppliers.Update(supplier);
    }
    /// <inheritdoc/>
    public System.Threading.Tasks.Task SaveChangesAsync(
        System.Threading.CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
