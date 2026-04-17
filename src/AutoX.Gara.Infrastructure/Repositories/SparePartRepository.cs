using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Domain.Entities.Inventory;

using AutoX.Gara.Infrastructure.Database;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Models;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;



using AutoX.Gara.Application.Abstractions.Repositories;



namespace AutoX.Gara.Infrastructure.Repositories;



/// <summary>

/// Repository for managing Part entities.

/// Provides CRUD operations and advanced querying capabilities.

/// </summary>

public sealed class PartRepository
 : IPartRepository
{

    private readonly AutoXDbContext _dbContext;



    /// <summary>

    /// Initializes a new instance of the PartRepository class.

    /// </summary>

    public PartRepository(AutoXDbContext dbContext) => _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));



    /// <summary>

    /// Retrieves a paginated list of parts with filtering and sorting.

    /// </summary>

    public async Task<(List<Part> items, int totalCount)> GetPageAsync(PartListQuery query)

    {

        query.Validate();



        IQueryable<Part> q = _dbContext.Parts.AsQueryable();



        // Apply filters

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))

        {

            string searchLower = query.SearchTerm.ToLower();

            q = q.Where(p => p.PartName.ToLower().Contains(searchLower) ||

                              p.PartCode.ToLower().Contains(searchLower) ||

                              p.Manufacturer.ToLower().Contains(searchLower));

        }



        if (query.FilterSupplierId.HasValue)

        {

            q = q.Where(p => p.SupplierId == query.FilterSupplierId.Value);

        }



        if (query.FilterCategory.HasValue)

        {

            q = q.Where(p => p.PartCategory == query.FilterCategory.Value);

        }



        if (query.FilterInStock.HasValue)

        {

            q = query.FilterInStock.Value

                ? q.Where(p => p.InventoryQuantity > 0)

                : q.Where(p => p.InventoryQuantity <= 0);

        }



        if (query.FilterDefective.HasValue)

        {

            q = q.Where(p => p.IsDefective == query.FilterDefective.Value);

        }



        if (query.FilterExpired.HasValue)

        {

            q = query.FilterExpired.Value

                ? q.Where(p => p.ExpiryDate.HasValue &&

                                  DateOnly.FromDateTime(DateTime.UtcNow) > p.ExpiryDate.Value)

                : q.Where(p => !p.ExpiryDate.HasValue ||

                                  DateOnly.FromDateTime(DateTime.UtcNow) <= p.ExpiryDate.Value);

        }



        if (query.FilterDiscontinued.HasValue)

        {

            q = q.Where(p => p.IsDiscontinued == query.FilterDiscontinued.Value);

        }



        // Count total before pagination

        int totalCount = await q.CountAsync();



        // Apply sorting

        q = query.SortBy switch

        {

            PartSortField.PartName => query.SortDescending

                ? q.OrderByDescending(p => p.PartName)

                : q.OrderBy(p => p.PartName),



            PartSortField.PurchasePrice => query.SortDescending

                ? q.OrderByDescending(p => p.PurchasePrice)

                : q.OrderBy(p => p.PurchasePrice),



            PartSortField.SellingPrice => query.SortDescending

                ? q.OrderByDescending(p => p.SellingPrice)

                : q.OrderBy(p => p.SellingPrice),



            PartSortField.InventoryQuantity => query.SortDescending

                ? q.OrderByDescending(p => p.InventoryQuantity)

                : q.OrderBy(p => p.InventoryQuantity),



            PartSortField.DateAdded => query.SortDescending

                ? q.OrderByDescending(p => p.DateAdded)

                : q.OrderBy(p => p.DateAdded),



            PartSortField.ExpiryDate => query.SortDescending

                ? q.OrderByDescending(p => p.ExpiryDate)

                : q.OrderBy(p => p.ExpiryDate),



            PartSortField.TotalValue => query.SortDescending

                ? q.OrderByDescending(p => p.InventoryQuantity * p.PurchasePrice)

                : q.OrderBy(p => p.InventoryQuantity * p.PurchasePrice),



            _ => q.OrderBy(p => p.PartName)

        };



        // Apply pagination

        List<Part> items = await q

            .Skip((query.Page - 1) * query.PageSize)

            .Take(query.PageSize)

            .ToListAsync();



        return (items, totalCount);

    }



    /// <summary>

    /// Retrieves a part by identifier.

    /// </summary>

    public async Task<Part> GetByIdAsync(int id)

    {

        return await _dbContext.Parts

            .FirstOrDefaultAsync(p => p.Id == id);

    }



    /// <summary>

    /// Checks if a part with the given code exists.

    /// </summary>

    public async Task<bool> ExistsByPartCodeAsync(string partCode)

    {

        return await _dbContext.Parts

            .AnyAsync(p => p.PartCode == partCode);

    }



    /// <summary>

    /// Adds a new part to the repository.

    /// </summary>

    public async Task AddAsync(Part part)

    {

        System.ArgumentNullException.ThrowIfNull(part);

        await _dbContext.Parts.AddAsync(part);

    }



    /// <summary>

    /// Updates an existing part.

    /// </summary>

    public void Update(Part part)

    {

        System.ArgumentNullException.ThrowIfNull(part);

        _dbContext.Parts.Update(part);

    }



    /// <summary>

    /// Deletes a part from the repository.

    /// </summary>

    public void Delete(Part part)

    {

        System.ArgumentNullException.ThrowIfNull(part);

        _dbContext.Parts.Remove(part);

    }



    /// <summary>

    /// Saves all changes to the database.

    /// </summary>

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();

}

