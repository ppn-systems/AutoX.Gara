// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Domain.Enums.Employees;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AutoX.Gara.Infrastructure.Repositories;

public sealed class EmployeeSalaryRepository(AutoXDbContext context)
{
    private readonly AutoXDbContext _context = context
        ?? throw new System.ArgumentNullException(nameof(context));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EF Core translates ToLower() to SQL LOWER()")]
    public async System.Threading.Tasks.Task<(System.Collections.Generic.List<EmployeeSalary> Items, System.Int32 TotalCount)> GetPageAsync(
        EmployeeSalaryListQuery query,
        System.Threading.CancellationToken ct = default)
    {
        query.Validate();

        IQueryable<EmployeeSalary> q = _context.EmployeeSalaries.AsNoTracking();

        if (query.FilterEmployeeId.HasValue && query.FilterEmployeeId.Value > 0)
        {
            q = q.Where(es => es.EmployeeId == query.FilterEmployeeId.Value);
        }

        if (query.FilterSalaryType.HasValue && query.FilterSalaryType.Value != SalaryType.None)
        {
            q = q.Where(es => es.SalaryType == query.FilterSalaryType.Value);
        }

        if (query.FilterFromDate.HasValue)
        {
            q = q.Where(es => es.EffectiveFrom >= query.FilterFromDate.Value);
        }

        if (query.FilterToDate.HasValue)
        {
            q = q.Where(es => (es.EffectiveTo ?? System.DateTime.MaxValue) <= query.FilterToDate.Value);
        }

        if (!System.String.IsNullOrWhiteSpace(query.SearchTerm))
        {
            System.String term = query.SearchTerm.Trim().ToLowerInvariant();
            q = q.Where(es => es.Note != null && es.Note.ToLower().Contains(term));
        }

        q = (query.SortBy, query.SortDescending) switch
        {
            (EmployeeSalarySortField.EffectiveFrom, false) => q.OrderBy(es => es.EffectiveFrom),
            (EmployeeSalarySortField.EffectiveFrom, true) => q.OrderByDescending(es => es.EffectiveFrom),
            (EmployeeSalarySortField.Salary, false) => q.OrderBy(es => es.Salary),
            (EmployeeSalarySortField.Salary, true) => q.OrderByDescending(es => es.Salary),
            (EmployeeSalarySortField.SalaryType, false) => q.OrderBy(es => es.SalaryType),
            (EmployeeSalarySortField.SalaryType, true) => q.OrderByDescending(es => es.SalaryType),
            _ => q.OrderByDescending(es => es.EffectiveFrom)
        };

        System.Int32 total = await q.CountAsync(ct).ConfigureAwait(false);

        System.Collections.Generic.List<EmployeeSalary> items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return (items, total);
    }

    public System.Threading.Tasks.Task<EmployeeSalary?> GetByIdAsync(System.Int32 id, System.Threading.CancellationToken ct = default)
        => _context.EmployeeSalaries.FirstOrDefaultAsync(es => es.Id == id, ct);

    public System.Threading.Tasks.Task AddAsync(EmployeeSalary data, System.Threading.CancellationToken ct = default)
        => _context.EmployeeSalaries.AddAsync(data, ct).AsTask();

    public void Update(EmployeeSalary data) => _context.EmployeeSalaries.Update(data);

    public void Remove(EmployeeSalary data) => _context.EmployeeSalaries.Remove(data);

    public System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}

