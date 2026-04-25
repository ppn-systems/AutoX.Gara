using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Contracts.Models;
using System.Collections.Generic;
namespace AutoX.Gara.Application.Repositories;
public interface IPartRepository
{
    System.Threading.Tasks.Task<(List<Part> items, int totalCount)> GetPageAsync(PartListQuery query);
    System.Threading.Tasks.Task<Part> GetByIdAsync(int id);
    System.Threading.Tasks.Task<bool> ExistsByPartCodeAsync(string partCode);
    System.Threading.Tasks.Task AddAsync(Part part);
    void Update(Part part);
    void Delete(Part part);
    System.Threading.Tasks.Task SaveChangesAsync();
}


