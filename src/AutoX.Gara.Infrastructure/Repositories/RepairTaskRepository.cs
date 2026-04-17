using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Domain.Entities.Repairs;

using AutoX.Gara.Infrastructure.Database;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Models;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;



using AutoX.Gara.Application.Abstractions.Repositories;



namespace AutoX.Gara.Infrastructure.Repositories;



public sealed class RepairTaskRepository
 : IRepairTaskRepository
{

    private readonly AutoXDbContext _dbContext;



    public RepairTaskRepository(AutoXDbContext dbContext)

        => _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));



    public async Task<(List<RepairTask> Items, int TotalCount)> GetPageAsync(RepairTaskListQuery query)

    {

        System.ArgumentNullException.ThrowIfNull(query);

        query.Validate();



        IQueryable<RepairTask> q = _dbContext.RepairTasks.AsNoTracking();



        if (!string.IsNullOrWhiteSpace(query.SearchTerm))

        {

            // Join to ServiceItem description for simple searching.

            string term = query.SearchTerm.Trim().ToLowerInvariant();

            q = q.Include(t => t.ServiceItem)

                 .Where(t => t.ServiceItem.Description != null && t.ServiceItem.Description.ToLower().Contains(term));

        }



        if (query.FilterRepairOrderId.HasValue)

        {

            q = q.Where(t => t.RepairOrderId == query.FilterRepairOrderId.Value);

        }



        if (query.FilterEmployeeId.HasValue)

        {

            q = q.Where(t => t.EmployeeId == query.FilterEmployeeId.Value);

        }



        if (query.FilterServiceItemId.HasValue)

        {

            q = q.Where(t => t.ServiceItemId == query.FilterServiceItemId.Value);

        }



        if (query.FilterStatus.HasValue)

        {

            q = q.Where(t => t.Status == query.FilterStatus.Value);

        }



        if (query.FilterFromDate.HasValue)

        {

            q = q.Where(t => t.StartDate >= query.FilterFromDate.Value);

        }



        if (query.FilterToDate.HasValue)

        {

            q = q.Where(t => t.StartDate <= query.FilterToDate.Value);

        }



        q = (query.SortBy, query.SortDescending) switch

        {

            (RepairTaskSortField.Id, false) => q.OrderBy(t => t.Id),

            (RepairTaskSortField.Id, true) => q.OrderByDescending(t => t.Id),

            (RepairTaskSortField.RepairOrderId, false) => q.OrderBy(t => t.RepairOrderId),

            (RepairTaskSortField.RepairOrderId, true) => q.OrderByDescending(t => t.RepairOrderId),

            (RepairTaskSortField.Status, false) => q.OrderBy(t => t.Status),

            (RepairTaskSortField.Status, true) => q.OrderByDescending(t => t.Status),

            (RepairTaskSortField.StartDate, false) => q.OrderBy(t => t.StartDate),

            (RepairTaskSortField.StartDate, true) => q.OrderByDescending(t => t.StartDate),

            (RepairTaskSortField.CompletionDate, false) => q.OrderBy(t => t.CompletionDate),

            (RepairTaskSortField.CompletionDate, true) => q.OrderByDescending(t => t.CompletionDate),

            _ => q.OrderByDescending(t => t.Id)

        };



        int totalCount = await q.CountAsync().ConfigureAwait(false);



        List<RepairTask> items = await q

            .Skip((query.Page - 1) * query.PageSize)

            .Take(query.PageSize)

            .ToListAsync()

            .ConfigureAwait(false);



        return (items, totalCount);

    }



    public Task<RepairTask> GetByIdAsync(int id)

        => _dbContext.RepairTasks.FirstOrDefaultAsync(t => t.Id == id);



    public Task AddAsync(RepairTask task)

    {

        System.ArgumentNullException.ThrowIfNull(task);

        return _dbContext.RepairTasks.AddAsync(task).AsTask();

    }



    public void Update(RepairTask task)

    {

        System.ArgumentNullException.ThrowIfNull(task);

        _dbContext.RepairTasks.Update(task);

    }



    public void Delete(RepairTask task)

    {

        System.ArgumentNullException.ThrowIfNull(task);

        _dbContext.RepairTasks.Remove(task);

    }



    public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();

}

