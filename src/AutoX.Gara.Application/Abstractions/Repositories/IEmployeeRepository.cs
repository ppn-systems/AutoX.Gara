using AutoX.Gara.Domain.Entities.Identity;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IEmployeeRepository
{
    System.Threading.Tasks.Task<(System.Collections.Generic.List<Employee> Items, System.Int32 TotalCount)> GetPageAsync(EmployeeListQuery query, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<Employee> GetByIdAsync(System.Int32 id, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<System.Boolean> ExistsByEmailAsync(System.String email, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<System.Boolean> ExistsByPhoneAsync(System.String phone, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(Employee employee, System.Threading.CancellationToken ct = default);
    void Update(Employee employee);
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}
