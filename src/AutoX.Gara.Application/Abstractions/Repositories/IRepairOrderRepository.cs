using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IRepairOrderRepository
{
    System.Threading.Tasks.Task<(System.Collections.Generic.List<RepairOrder> Items, System.Int32 TotalCount)> GetPageAsync(RepairOrderListQuery query);
    System.Threading.Tasks.Task<RepairOrder> GetByIdAsync(System.Int32 id);
    System.Threading.Tasks.Task AddAsync(RepairOrder repairOrder);
    void Update(RepairOrder repairOrder);
    void Delete(RepairOrder repairOrder);
    System.Threading.Tasks.Task SaveChangesAsync();
}
