using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Contracts.Models;
using System.Collections.Generic;
namespace AutoX.Gara.Application.Repositories;
public interface IRepairTaskRepository
{
    System.Threading.Tasks.Task<(List<RepairTask> Items, int TotalCount)> GetPageAsync(RepairTaskListQuery query);
    System.Threading.Tasks.Task<RepairTask> GetByIdAsync(int id);
    System.Threading.Tasks.Task AddAsync(RepairTask task);
    void Update(RepairTask task);
    void Delete(RepairTask task);
    System.Threading.Tasks.Task SaveChangesAsync();
}


