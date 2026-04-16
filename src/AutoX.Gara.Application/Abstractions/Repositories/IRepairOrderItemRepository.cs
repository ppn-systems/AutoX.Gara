using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IRepairOrderItemRepository
{
    System.Threading.Tasks.Task<(System.Collections.Generic.List<RepairOrderItem> Items, System.Int32 TotalCount)> GetPageAsync(RepairOrderItemListQuery query);
    System.Threading.Tasks.Task<RepairOrderItem> GetByIdAsync(System.Int32 id);
    System.Threading.Tasks.Task AddAsync(RepairOrderItem item);
    void Update(RepairOrderItem item);
    void Delete(RepairOrderItem item);
    System.Threading.Tasks.Task SaveChangesAsync();
}
