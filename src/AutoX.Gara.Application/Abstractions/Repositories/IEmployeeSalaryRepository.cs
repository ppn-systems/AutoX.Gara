using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IEmployeeSalaryRepository
{
    System.Threading.Tasks.Task<(System.Collections.Generic.List<EmployeeSalary> Items, System.Int32 TotalCount)> GetPageAsync(EmployeeSalaryListQuery query, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<EmployeeSalary> GetByIdAsync(System.Int32 id, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(EmployeeSalary data, System.Threading.CancellationToken ct = default);
    void Update(EmployeeSalary data);
    void Remove(EmployeeSalary data);
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}
