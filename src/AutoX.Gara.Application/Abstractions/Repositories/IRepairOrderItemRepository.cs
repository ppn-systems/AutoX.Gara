using AutoX.Gara.Domain.Entities.Repairs;
using AutoX.Gara.Contracts.Models;
using System.Collections.Generic;
namespace AutoX.Gara.Application.Abstractions.Repositories;
public interface IRepairOrderItemRepository
{
    System.Threading.Tasks.Task<(List<RepairOrderItem> Items, int TotalCount)> GetPageAsync(RepairOrderItemListQuery query);
    System.Threading.Tasks.Task<RepairOrderItem> GetByIdAsync(int id);
    System.Threading.Tasks.Task AddAsync(RepairOrderItem item);
    void Update(RepairOrderItem item);
    void Delete(RepairOrderItem item);
    System.Threading.Tasks.Task<(List<RepairOrderItem> Items, int TotalCount)> GetByOrderIdAsync(int orderId);
    System.Threading.Tasks.Task SaveChangesAsync();
}

