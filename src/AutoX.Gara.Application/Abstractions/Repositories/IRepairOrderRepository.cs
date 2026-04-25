using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Contracts.Models;
using System.Collections.Generic;
namespace AutoX.Gara.Application.Repositories;
public interface IRepairOrderRepository
{
    System.Threading.Tasks.Task<(List<RepairOrder> Items, int TotalCount)> GetPageAsync(RepairOrderListQuery query);
    System.Threading.Tasks.Task<RepairOrder> GetByIdAsync(int id);
    System.Threading.Tasks.Task AddAsync(RepairOrder repairOrder);
    void Update(RepairOrder repairOrder);
    void Delete(RepairOrder repairOrder);
    System.Threading.Tasks.Task SaveChangesAsync();
}


