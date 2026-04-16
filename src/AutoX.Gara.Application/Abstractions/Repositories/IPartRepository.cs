using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IPartRepository
{
    System.Threading.Tasks.Task<(System.Collections.Generic.List<Part> items, System.Int32 totalCount)> GetPageAsync(PartListQuery query);
    System.Threading.Tasks.Task<Part> GetByIdAsync(System.Int32 id);
    System.Threading.Tasks.Task<System.Boolean> ExistsByPartCodeAsync(System.String partCode);
    System.Threading.Tasks.Task AddAsync(Part part);
    void Update(Part part);
    void Delete(Part part);
    System.Threading.Tasks.Task SaveChangesAsync();
}
