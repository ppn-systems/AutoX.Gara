using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface ISupplierRepository
{
    System.Threading.Tasks.Task<(System.Collections.Generic.List<Supplier> Items, System.Int32 TotalCount)> GetPageAsync(SupplierListQuery query, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<Supplier> GetByIdAsync(System.Int32 id, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<System.Boolean> ExistsByContactAsync(System.String email, System.String taxCode, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(Supplier supplier, System.Threading.CancellationToken ct = default);
    void Update(Supplier supplier);
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}
