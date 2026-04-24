using AutoX.Gara.Application.Abstractions.Repositories;
// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace AutoX.Gara.Infrastructure.Repositories;



/// <summary>

/// Repository for managing Invoice entities.

/// </summary>

public sealed class InvoiceRepository
 : IInvoiceRepository
{

    private readonly AutoXDbContext _dbContext;



    public InvoiceRepository(AutoXDbContext dbContext)

        => _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));



    /// <summary>

    /// Loads the Invoice entity with all details using tracking.

    /// Tracking is needed for financial recalculation and avoids duplicate entity instances when materializing large object graphs.

    /// Use this when identity resolution is required.

    /// </summary>

    /// <param name="id">The invoice identifier.</param>

    /// <returns>A Task of Invoice with details, or null if not found.</returns>

    public Task<Invoice> GetInvoiceWithFullGraphTrackedAsync(int id)

    {

        // AutoXDbContextFactory configures QueryTrackingBehavior.NoTracking globally.

        // For financial recalculation we must track (identity resolution) to avoid

        // duplicate entity instances (e.g., Part) when materializing large graphs.

        return _dbContext.Invoices

            .AsTracking()

            .AsSplitQuery()

            .Include(i => i.Transactions)

            .Include(i => i.RepairOrders)

                .ThenInclude(ro => ro.Tasks)

                    .ThenInclude(t => t.ServiceItem)

            .Include(i => i.RepairOrders)

                .ThenInclude(ro => ro.Parts)

                    .ThenInclude(p => p.SparePart)

            .FirstOrDefaultAsync(i => i.Id == id);

    }



    public async Task<(List<Invoice> Items, int TotalCount)> GetPageAsync(

        InvoiceListQuery query)

    {

        System.ArgumentNullException.ThrowIfNull(query);

        query.Validate();



        IQueryable<Invoice> q = _dbContext.Invoices.AsNoTracking();



        if (!string.IsNullOrWhiteSpace(query.SearchTerm))

        {

            string term = query.SearchTerm.Trim().ToLowerInvariant();

            q = q.Where(i => i.InvoiceNumber != null && i.InvoiceNumber.ToLower().Contains(term));

        }



        if (query.FilterCustomerId.HasValue)

        {

            q = q.Where(i => i.CustomerId == query.FilterCustomerId.Value);

        }



        if (query.FilterPaymentStatus.HasValue)

        {

            q = q.Where(i => i.PaymentStatus == query.FilterPaymentStatus.Value);

        }



        if (query.FilterFromDate.HasValue)

        {

            q = q.Where(i => i.InvoiceDate >= query.FilterFromDate.Value);

        }



        if (query.FilterToDate.HasValue)

        {

            q = q.Where(i => i.InvoiceDate <= query.FilterToDate.Value);

        }



        q = (query.SortBy, query.SortDescending) switch

        {

            (InvoiceSortField.InvoiceDate, false) => q.OrderBy(i => i.InvoiceDate),

            (InvoiceSortField.InvoiceDate, true) => q.OrderByDescending(i => i.InvoiceDate),

            (InvoiceSortField.InvoiceNumber, false) => q.OrderBy(i => i.InvoiceNumber),

            (InvoiceSortField.InvoiceNumber, true) => q.OrderByDescending(i => i.InvoiceNumber),

            (InvoiceSortField.TotalAmount, false) => q.OrderBy(i => i.TotalAmount),

            (InvoiceSortField.TotalAmount, true) => q.OrderByDescending(i => i.TotalAmount),

            (InvoiceSortField.BalanceDue, false) => q.OrderBy(i => i.BalanceDue),

            (InvoiceSortField.BalanceDue, true) => q.OrderByDescending(i => i.BalanceDue),

            (InvoiceSortField.PaymentStatus, false) => q.OrderBy(i => i.PaymentStatus),

            (InvoiceSortField.PaymentStatus, true) => q.OrderByDescending(i => i.PaymentStatus),

            _ => q.OrderByDescending(i => i.InvoiceDate)

        };



        int totalCount = await q.CountAsync().ConfigureAwait(false);



        List<Invoice> items = await q

            .Skip((query.Page - 1) * query.PageSize)

            .Take(query.PageSize)

            .ToListAsync()

            .ConfigureAwait(false);



        return (items, totalCount);

    }



    public Task<Invoice> GetByIdAsync(int id)

        => _dbContext.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);



    public Task<bool> ExistsByInvoiceNumberAsync(string invoiceNumber, int? excludeId = null)

    {

        System.ArgumentNullException.ThrowIfNull(invoiceNumber);

        string normalized = invoiceNumber.Trim();



        IQueryable<Invoice> q = _dbContext.Invoices;

        if (excludeId.HasValue)

        {

            q = q.Where(i => i.Id != excludeId.Value);

        }



        return q.AnyAsync(i => i.InvoiceNumber == normalized);

    }



    /// <summary>

    /// Loads an invoice with related data needed for financial recalculation.

    /// Tracking query.

    /// </summary>

    public Task<Invoice> GetByIdWithDetailsAsync(int id)

    {

        return _dbContext.Invoices

            .Include(i => i.Transactions)

            .Include(i => i.RepairOrders)

                .ThenInclude(ro => ro.Tasks)

                    .ThenInclude(t => t.ServiceItem)

            .Include(i => i.RepairOrders)

                .ThenInclude(ro => ro.Parts)

                    .ThenInclude(p => p.SparePart)

            .FirstOrDefaultAsync(i => i.Id == id);

    }



    public Task AddAsync(Invoice invoice)

    {

        System.ArgumentNullException.ThrowIfNull(invoice);

        return _dbContext.Invoices.AddAsync(invoice).AsTask();

    }



    public void Update(Invoice invoice)

    {

        System.ArgumentNullException.ThrowIfNull(invoice);

        _dbContext.Invoices.Update(invoice);

    }



    public void Delete(Invoice invoice)

    {

        System.ArgumentNullException.ThrowIfNull(invoice);

        _dbContext.Invoices.Remove(invoice);

    }



    public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();

}

