using AutoX.Gara.Shared.Enums;
using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.



using AutoX.Gara.Domain.Entities.Identity;

using AutoX.Gara.Domain.Enums;

using AutoX.Gara.Domain.Enums.Employees;

using AutoX.Gara.Infrastructure.Database;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Models;

using Microsoft.EntityFrameworkCore;

using System.Linq;



using AutoX.Gara.Application.Abstractions.Repositories;



namespace AutoX.Gara.Infrastructure.Repositories;



/// <summary>

/// EF Core implementation c?a repository cho Employee.

/// </summary>

public sealed class EmployeeRepository(AutoXDbContext context) : IEmployeeRepository

{

    private readonly AutoXDbContext _context = context

        ?? throw new System.ArgumentNullException(nameof(context));



    // --- Query ----------------------------------------------------------------



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EF Core translates ToLower() to SQL LOWER()")]

    public async System.Threading.Tasks.Task<(List<Employee> Items, int TotalCount)> GetPageAsync(

        EmployeeListQuery query,

        System.Threading.CancellationToken ct = default)

    {

        IQueryable<Employee> q = _context.Employees.AsNoTracking();



        // -- Search ---------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))

        {

            string term = query.SearchTerm.Trim().ToLowerInvariant();

            q = q.Where(e =>

                (e.Name != null && e.Name.ToLower().Contains(term)) ||

                (e.Email != null && e.Email.ToLower().Contains(term)) ||

                (e.PhoneNumber != null && e.PhoneNumber.Contains(term)));

        }



        // -- Filter ---------------------------------------------------------

        if (query.FilterPosition != Position.None)

        {

            q = q.Where(e => e.Position == query.FilterPosition);

        }



        if (query.FilterStatus != EmploymentStatus.None)

        {

            q = q.Where(e => e.Status == query.FilterStatus);

        }



        if (query.FilterGender != Gender.None)

        {

            q = q.Where(e => e.Gender == query.FilterGender);

        }



        // -- Sort -----------------------------------------------------------

        q = (query.SortBy, query.SortDescending) switch

        {

            (EmployeeSortField.Name, false) => q.OrderBy(e => e.Name),

            (EmployeeSortField.Name, true) => q.OrderByDescending(e => e.Name),

            (EmployeeSortField.Email, false) => q.OrderBy(e => e.Email),

            (EmployeeSortField.Email, true) => q.OrderByDescending(e => e.Email),

            (EmployeeSortField.Position, false) => q.OrderBy(e => e.Position),

            (EmployeeSortField.Position, true) => q.OrderByDescending(e => e.Position),

            (EmployeeSortField.Status, false) => q.OrderBy(e => e.Status),

            (EmployeeSortField.Status, true) => q.OrderByDescending(e => e.Status),

            (EmployeeSortField.StartDate, false) => q.OrderBy(e => e.StartDate),

            (EmployeeSortField.StartDate, true) => q.OrderByDescending(e => e.StartDate),

            (EmployeeSortField.Gender, false) => q.OrderBy(e => e.Gender),

            (EmployeeSortField.Gender, true) => q.OrderByDescending(e => e.Gender),

            _ => q.OrderBy(e => e.Name)

        };



        // -- Count + Page ---------------------------------------------------

        int totalCount = await q.CountAsync(ct).ConfigureAwait(false);



        List<Employee> items = await q

            .Skip((query.Page - 1) * query.PageSize)

            .Take(query.PageSize)

            .ToListAsync(ct)

            .ConfigureAwait(false);



        return (items, totalCount);

    }



    public System.Threading.Tasks.Task<Employee> GetByIdAsync(

        int id,

        System.Threading.CancellationToken ct = default)

        => _context.Employees

            .AsNoTracking()

            .FirstOrDefaultAsync(e => e.Id == id, ct);



    public System.Threading.Tasks.Task<bool> ExistsByEmailAsync(

        string email,

        System.Threading.CancellationToken ct = default)

        => _context.Employees.AnyAsync(e => e.Email == email, ct);



    public System.Threading.Tasks.Task<bool> ExistsByPhoneAsync(

        string phone,

        System.Threading.CancellationToken ct = default)

        => _context.Employees.AnyAsync(e => e.PhoneNumber == phone, ct);



    // --- Write ----------------------------------------------------------------



    public System.Threading.Tasks.Task AddAsync(

        Employee employee,

        System.Threading.CancellationToken ct = default)

        => _context.Employees.AddAsync(employee, ct).AsTask();



    public void Update(Employee employee)

        => _context.Employees.Update(employee);

    public void Delete(Employee employee)

        => _context.Employees.Remove(employee);



    public System.Threading.Tasks.Task SaveChangesAsync(

        System.Threading.CancellationToken ct = default)

        => _context.SaveChangesAsync(ct);

}

