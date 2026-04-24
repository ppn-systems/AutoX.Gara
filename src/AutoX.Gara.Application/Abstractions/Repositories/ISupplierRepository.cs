using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Contracts.Models;
using System.Collections.Generic;
namespace AutoX.Gara.Application.Abstractions.Repositories;
public interface ISupplierRepository
{
    System.Threading.Tasks.Task<(List<Supplier> Items, int TotalCount)> GetPageAsync(SupplierListQuery query, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<Supplier> GetByIdAsync(int id, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<bool> ExistsByDetailsAsync(string name, string email, string phoneNumber, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(Supplier supplier, System.Threading.CancellationToken ct = default);
    void Update(Supplier supplier);
    void Delete(Supplier supplier);
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}

