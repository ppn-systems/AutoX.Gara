using AutoX.Gara.Domain.Entities.Customers;
using System.Collections.Generic;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IVehicleRepository
{
    System.Threading.Tasks.Task<Vehicle> GetByIdAsync(int id, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<(List<Vehicle> Items, int TotalCount)> GetByCustomerIdAsync(int customerId, int page, int pageSize, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<bool> ExistsAsync(string licensePlate, string engineNumber = null, string frameNumber = null, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(Vehicle vehicle, System.Threading.CancellationToken ct = default);
    void Update(Vehicle vehicle);
    void Delete(Vehicle vehicle);
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}
