using System;
using System.Collections.Generic;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Infrastructure.Abstractions.Repositories;


/// <summary>
/// Abstraction for employee repository.
/// </summary>
public interface IEmployeeRepository
{
    System.Threading.Tasks.Task<(List<Employee> Items, int TotalCount)> GetPageAsync(
        EmployeeListQuery query,
        System.Threading.CancellationToken ct = default);

    System.Threading.Tasks.Task<Employee> GetByIdAsync(int id, System.Threading.CancellationToken ct = default);

    System.Threading.Tasks.Task<bool> ExistsByEmailAsync(string email, System.Threading.CancellationToken ct = default);

    System.Threading.Tasks.Task<bool> ExistsByPhoneAsync(string phone, System.Threading.CancellationToken ct = default);

    System.Threading.Tasks.Task AddAsync(Employee employee, System.Threading.CancellationToken ct = default);

    void Update(Employee employee);

    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}