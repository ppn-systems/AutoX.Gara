using AutoX.Gara.Application.Abstractions.Repositories;
// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace AutoX.Gara.Infrastructure.Repositories;



public sealed class RepairOrderItemRepository
 : IRepairOrderItemRepository
{

    private readonly AutoXDbContext _dbContext;



    public RepairOrderItemRepository(AutoXDbContext dbContext)

        => _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));



    public async Task<(List<RepairOrderItem> Items, int TotalCount)> GetPageAsync(RepairOrderItemListQuery query)

    {

        System.ArgumentNullException.ThrowIfNull(query);

        query.Validate();



        IQueryable<RepairOrderItem> q = _dbContext.RepairOrderItems.AsNoTracking().Where(i => i.DeletedAt == null);



        if (!string.IsNullOrWhiteSpace(query.SearchTerm))

        {

            // Join to Part name for simple searching.

            string term = query.SearchTerm.Trim().ToLowerInvariant();

            q = q.Include(i => i.SparePart)

                 .Where(i => i.SparePart.PartName != null && i.SparePart.PartName.ToLower().Contains(term));

        }



        if (query.FilterRepairOrderId.HasValue)

        {

            q = q.Where(i => i.RepairOrderId == query.FilterRepairOrderId.Value);

        }



        if (query.FilterPartId.HasValue)

        {

            q = q.Where(i => i.PartId == query.FilterPartId.Value);

        }



        q = (query.SortBy, query.SortDescending) switch

        {

            (RepairOrderItemSortField.Id, false) => q.OrderBy(i => i.Id),

            (RepairOrderItemSortField.Id, true) => q.OrderByDescending(i => i.Id),

            (RepairOrderItemSortField.RepairOrderId, false) => q.OrderBy(i => i.RepairOrderId),

            (RepairOrderItemSortField.RepairOrderId, true) => q.OrderByDescending(i => i.RepairOrderId),

            (RepairOrderItemSortField.PartId, false) => q.OrderBy(i => i.PartId),

            (RepairOrderItemSortField.PartId, true) => q.OrderByDescending(i => i.PartId),

            (RepairOrderItemSortField.Quantity, false) => q.OrderBy(i => i.Quantity),

            (RepairOrderItemSortField.Quantity, true) => q.OrderByDescending(i => i.Quantity),

            _ => q.OrderByDescending(i => i.Id)

        };



        int totalCount = await q.CountAsync().ConfigureAwait(false);



        List<RepairOrderItem> items = await q

            .Skip((query.Page - 1) * query.PageSize)

            .Take(query.PageSize)

            .ToListAsync()

            .ConfigureAwait(false);



        return (items, totalCount);

    }



    public Task<RepairOrderItem> GetByIdAsync(int id)

        => _dbContext.RepairOrderItems.FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null);



    public Task AddAsync(RepairOrderItem item)

    {

        System.ArgumentNullException.ThrowIfNull(item);

        return _dbContext.RepairOrderItems.AddAsync(item).AsTask();

    }



    public void Update(RepairOrderItem item)

    {

        System.ArgumentNullException.ThrowIfNull(item);

        _dbContext.RepairOrderItems.Update(item);

    }



    public void Delete(RepairOrderItem item)

    {

        System.ArgumentNullException.ThrowIfNull(item);

        item.DeletedAt = System.DateTime.UtcNow;

        _dbContext.RepairOrderItems.Update(item);

    }



    public async Task<(List<RepairOrderItem> Items, int TotalCount)> GetByOrderIdAsync(int orderId)
    {
        var items = await _dbContext.RepairOrderItems
            .Include(i => i.SparePart)
            .Where(i => i.RepairOrderId == orderId && i.DeletedAt == null)
            .ToListAsync()
            .ConfigureAwait(false);
        return (items, items.Count);
    }

    public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();

}

