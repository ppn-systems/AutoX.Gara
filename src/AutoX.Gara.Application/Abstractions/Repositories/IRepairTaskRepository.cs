using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IRepairTaskRepository
{
    System.Threading.Tasks.Task<(System.Collections.Generic.List<RepairTask> Items, System.Int32 TotalCount)> GetPageAsync(RepairTaskListQuery query);
    System.Threading.Tasks.Task<RepairTask> GetByIdAsync(System.Int32 id);
    System.Threading.Tasks.Task AddAsync(RepairTask task);
    void Update(RepairTask task);
    void Delete(RepairTask task);
    System.Threading.Tasks.Task SaveChangesAsync();
}
