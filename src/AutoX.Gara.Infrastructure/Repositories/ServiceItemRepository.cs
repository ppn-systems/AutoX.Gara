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



public sealed class ServiceItemRepository
 : IServiceItemRepository
{

    private readonly AutoXDbContext _dbContext;



    public ServiceItemRepository(AutoXDbContext dbContext)

        => _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));



    public async Task<(List<ServiceItem> Items, int TotalCount)> GetPageAsync(ServiceItemListQuery query)

    {

        System.ArgumentNullException.ThrowIfNull(query);

        query.Validate();



        IQueryable<ServiceItem> q = _dbContext.ServiceItems.AsNoTracking();



        if (!string.IsNullOrWhiteSpace(query.SearchTerm))

        {

            string term = query.SearchTerm.Trim().ToLowerInvariant();

            q = q.Where(s => s.Description != null && s.Description.ToLower().Contains(term));

        }



        if (query.FilterType.HasValue)

        {

            q = q.Where(s => s.Type == query.FilterType.Value);

        }



        if (query.FilterMinUnitPrice.HasValue)

        {

            q = q.Where(s => s.UnitPrice >= query.FilterMinUnitPrice.Value);

        }



        if (query.FilterMaxUnitPrice.HasValue)

        {

            q = q.Where(s => s.UnitPrice <= query.FilterMaxUnitPrice.Value);

        }



        q = (query.SortBy, query.SortDescending) switch

        {

            (ServiceItemSortField.Description, false) => q.OrderBy(s => s.Description),

            (ServiceItemSortField.Description, true) => q.OrderByDescending(s => s.Description),

            (ServiceItemSortField.UnitPrice, false) => q.OrderBy(s => s.UnitPrice),

            (ServiceItemSortField.UnitPrice, true) => q.OrderByDescending(s => s.UnitPrice),

            (ServiceItemSortField.Type, false) => q.OrderBy(s => s.Type),

            (ServiceItemSortField.Type, true) => q.OrderByDescending(s => s.Type),

            _ => q.OrderBy(s => s.Description)

        };



        int totalCount = await q.CountAsync().ConfigureAwait(false);



        List<ServiceItem> items = await q

            .Skip((query.Page - 1) * query.PageSize)

            .Take(query.PageSize)

            .ToListAsync()

            .ConfigureAwait(false);



        return (items, totalCount);

    }



    public Task<ServiceItem> GetByIdAsync(int id)

        => _dbContext.ServiceItems.FirstOrDefaultAsync(s => s.Id == id);



    public Task AddAsync(ServiceItem serviceItem)

    {

        System.ArgumentNullException.ThrowIfNull(serviceItem);

        return _dbContext.ServiceItems.AddAsync(serviceItem).AsTask();

    }



    public void Update(ServiceItem serviceItem)

    {

        System.ArgumentNullException.ThrowIfNull(serviceItem);

        _dbContext.ServiceItems.Update(serviceItem);

    }



    public void Delete(ServiceItem serviceItem)

    {

        System.ArgumentNullException.ThrowIfNull(serviceItem);

        _dbContext.ServiceItems.Remove(serviceItem);

    }



    public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();

}

