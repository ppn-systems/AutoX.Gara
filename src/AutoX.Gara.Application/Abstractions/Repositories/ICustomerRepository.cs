using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Shared.Models;
using System.Collections.Generic;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface ICustomerRepository
{
    System.Threading.Tasks.Task<(List<Customer> Items, int TotalCount)> GetPageAsync(CustomerListQuery query, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<Customer> GetByIdAsync(int id, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<bool> ExistsByContactAsync(string email, string phoneNumber, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(Customer customer, System.Threading.CancellationToken ct = default);
    void Update(Customer customer);
    void Delete(Customer customer);
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}
