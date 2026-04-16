using AutoX.Gara.Domain.Entities.Customers;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IVehicleRepository
{
    System.Threading.Tasks.Task<Vehicle> GetByIdAsync(System.Int32 id, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<(System.Collections.Generic.List<Vehicle> Items, System.Int32 TotalCount)> GetByCustomerIdAsync(System.Int32 customerId, System.Int32 page, System.Int32 pageSize, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task<System.Boolean> ExistsAsync(System.String licensePlate, System.String engineNumber = null, System.String frameNumber = null, System.Threading.CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(Vehicle vehicle, System.Threading.CancellationToken ct = default);
    void Update(Vehicle vehicle);
    System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken ct = default);
}
