using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Application.Abstractions.Repositories;

using AutoX.Gara.Domain.Entities.Invoices;

using AutoX.Gara.Infrastructure.Database;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Models;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;



namespace AutoX.Gara.Infrastructure.Repositories;



public sealed class TransactionRepository
 : ITransactionRepository
{

    private readonly AutoXDbContext _dbContext;



    public TransactionRepository(AutoXDbContext dbContext)

        => _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));



    public async Task<(List<Transaction> Items, int TotalCount)> GetPageAsync(

        TransactionListQuery query)

    {

        System.ArgumentNullException.ThrowIfNull(query);

        query.Validate();



        IQueryable<Transaction> q = _dbContext.Transactions.AsNoTracking();



        if (!string.IsNullOrWhiteSpace(query.SearchTerm))

        {

            string term = query.SearchTerm.Trim().ToLowerInvariant();

            q = q.Where(t => t.Description != null && t.Description.ToLower().Contains(term));

        }



        if (query.FilterInvoiceId.HasValue)

        {

            q = q.Where(t => t.InvoiceId == query.FilterInvoiceId.Value);

        }



        if (query.FilterType.HasValue)

        {

            q = q.Where(t => t.Type == query.FilterType.Value);

        }



        if (query.FilterStatus.HasValue)

        {

            q = q.Where(t => t.Status == query.FilterStatus.Value);

        }



        if (query.FilterPaymentMethod.HasValue)

        {

            q = q.Where(t => t.PaymentMethod == query.FilterPaymentMethod.Value);

        }



        if (query.FilterMinAmount.HasValue)

        {

            q = q.Where(t => t.Amount >= query.FilterMinAmount.Value);

        }



        if (query.FilterMaxAmount.HasValue)

        {

            q = q.Where(t => t.Amount <= query.FilterMaxAmount.Value);

        }



        if (query.FilterFromDate.HasValue)

        {

            q = q.Where(t => t.TransactionDate >= query.FilterFromDate.Value);

        }



        if (query.FilterToDate.HasValue)

        {

            q = q.Where(t => t.TransactionDate <= query.FilterToDate.Value);

        }



        q = (query.SortBy, query.SortDescending) switch

        {

            (TransactionSortField.TransactionDate, false) => q.OrderBy(t => t.TransactionDate),

            (TransactionSortField.TransactionDate, true) => q.OrderByDescending(t => t.TransactionDate),

            (TransactionSortField.Amount, false) => q.OrderBy(t => t.Amount),

            (TransactionSortField.Amount, true) => q.OrderByDescending(t => t.Amount),

            (TransactionSortField.Status, false) => q.OrderBy(t => t.Status),

            (TransactionSortField.Status, true) => q.OrderByDescending(t => t.Status),

            (TransactionSortField.Type, false) => q.OrderBy(t => t.Type),

            (TransactionSortField.Type, true) => q.OrderByDescending(t => t.Type),

            (TransactionSortField.PaymentMethod, false) => q.OrderBy(t => t.PaymentMethod),

            (TransactionSortField.PaymentMethod, true) => q.OrderByDescending(t => t.PaymentMethod),

            _ => q.OrderByDescending(t => t.TransactionDate)

        };



        int totalCount = await q.CountAsync().ConfigureAwait(false);



        List<Transaction> items = await q

            .Skip((query.Page - 1) * query.PageSize)

            .Take(query.PageSize)

            .ToListAsync()

            .ConfigureAwait(false);



        return (items, totalCount);

    }



    public Task<Transaction> GetByIdAsync(int id)

        => _dbContext.Transactions.FirstOrDefaultAsync(t => t.Id == id);



    public Task AddAsync(Transaction transaction)

    {

        System.ArgumentNullException.ThrowIfNull(transaction);

        return _dbContext.Transactions.AddAsync(transaction).AsTask();

    }



    public void Update(Transaction transaction)

    {

        System.ArgumentNullException.ThrowIfNull(transaction);

        _dbContext.Transactions.Update(transaction);

    }



    public void Delete(Transaction transaction)

    {

        System.ArgumentNullException.ThrowIfNull(transaction);

        _dbContext.Transactions.Remove(transaction);

    }



    public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();

}

