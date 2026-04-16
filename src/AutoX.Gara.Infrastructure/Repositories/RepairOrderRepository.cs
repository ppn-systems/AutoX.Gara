using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Infrastructure.Database;
using Nalix.Common.Networking.Protocols;
using AutoX.Gara.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoX.Gara.Application.Abstractions.Repositories;

namespace AutoX.Gara.Infrastructure.Repositories;

public sealed class RepairOrderRepository : IRepairOrderRepository
{
    private readonly AutoXDbContext _dbContext;

    public RepairOrderRepository(AutoXDbContext dbContext)
        => _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));

    public async Task<(List<RepairOrder> Items, int TotalCount)> GetPageAsync(
        RepairOrderListQuery query)
    {
        System.ArgumentNullException.ThrowIfNull(query);
        query.Validate();

        IQueryable<RepairOrder> q = _dbContext.RepairOrders
            .AsNoTracking()
            .Include(ro => ro.Tasks)
                .ThenInclude(t => t.ServiceItem)
            .Include(ro => ro.Parts)
                .ThenInclude(p => p.SparePart);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            string term = query.SearchTerm.Trim();
            if (int.TryParse(term, out int id))
            {
                q = q.Where(ro => ro.Id == id);
            }
        }

        if (query.FilterCustomerId.HasValue)
        {
            q = q.Where(ro => ro.CustomerId == query.FilterCustomerId.Value);
        }

        if (query.FilterVehicleId.HasValue)
        {
            q = q.Where(ro => ro.VehicleId == query.FilterVehicleId.Value);
        }

        if (query.FilterInvoiceId.HasValue)
        {
            q = q.Where(ro => ro.InvoiceId == query.FilterInvoiceId.Value);
        }

        if (query.FilterStatus.HasValue)
        {
            q = q.Where(ro => ro.Status == query.FilterStatus.Value);
        }

        if (query.FilterFromDate.HasValue)
        {
            q = q.Where(ro => ro.OrderDate >= query.FilterFromDate.Value);
        }

        if (query.FilterToDate.HasValue)
        {
            q = q.Where(ro => ro.OrderDate <= query.FilterToDate.Value);
        }

        q = (query.SortBy, query.SortDescending) switch
        {
            (RepairOrderSortField.OrderDate, false) => q.OrderBy(ro => ro.OrderDate),
            (RepairOrderSortField.OrderDate, true) => q.OrderByDescending(ro => ro.OrderDate),
            (RepairOrderSortField.CompletionDate, false) => q.OrderBy(ro => ro.CompletionDate),
            (RepairOrderSortField.CompletionDate, true) => q.OrderByDescending(ro => ro.CompletionDate),
            (RepairOrderSortField.Status, false) => q.OrderBy(ro => ro.Status),
            (RepairOrderSortField.Status, true) => q.OrderByDescending(ro => ro.Status),
            _ => q.OrderByDescending(ro => ro.OrderDate)
        };

        int totalCount = await q.CountAsync().ConfigureAwait(false);

        List<RepairOrder> items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync()
            .ConfigureAwait(false);

        return (items, totalCount);
    }

    public Task<RepairOrder> GetByIdAsync(int id)
    {
        return _dbContext.RepairOrders
            .Include(ro => ro.Tasks)
                .ThenInclude(t => t.ServiceItem)
            .Include(ro => ro.Parts)
                .ThenInclude(p => p.SparePart)
            .FirstOrDefaultAsync(ro => ro.Id == id);
    }

    public Task AddAsync(RepairOrder repairOrder)
    {
        System.ArgumentNullException.ThrowIfNull(repairOrder);
        return _dbContext.RepairOrders.AddAsync(repairOrder).AsTask();
    }

    public void Update(RepairOrder repairOrder)
    {
        System.ArgumentNullException.ThrowIfNull(repairOrder);
        _dbContext.RepairOrders.Update(repairOrder);
    }

    public void Delete(RepairOrder repairOrder)
    {
        System.ArgumentNullException.ThrowIfNull(repairOrder);
        _dbContext.RepairOrders.Remove(repairOrder);
    }

    public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();
}