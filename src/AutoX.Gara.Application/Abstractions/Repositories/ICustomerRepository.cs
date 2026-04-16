using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface ICustomerRepository
{
    System.Threading.Tasks.Task<(System.Collections.Generic.List<Customer> Items, System.Int32 TotalCount)> GetPageAsync(CustomerListQuery query, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<Customer> GetByIdAsync(System.Int32 id, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<System.Boolean> ExistsByContactAsync(System.String email, System.String phoneNumber, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(Customer customer, System.Threading.CancellationToken ct = default);
    void Update(Customer customer);
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}
