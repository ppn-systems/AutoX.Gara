using System;
using System.Collections.Generic;
using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IEmployeeSalaryRepository
{
    System.Threading.Tasks.Task<(List<EmployeeSalary> Items, int TotalCount)> GetPageAsync(EmployeeSalaryListQuery query, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<EmployeeSalary> GetByIdAsync(int id, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(EmployeeSalary data, System.Threading.CancellationToken ct = default);
    void Update(EmployeeSalary data);
    void Delete(EmployeeSalary data);
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}