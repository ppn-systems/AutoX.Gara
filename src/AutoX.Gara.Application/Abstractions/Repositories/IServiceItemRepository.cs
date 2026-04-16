using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IServiceItemRepository
{
    System.Threading.Tasks.Task<(System.Collections.Generic.List<ServiceItem> Items, System.Int32 TotalCount)> GetPageAsync(ServiceItemListQuery query);
    System.Threading.Tasks.Task<ServiceItem> GetByIdAsync(System.Int32 id);
    System.Threading.Tasks.Task AddAsync(ServiceItem serviceItem);
    void Update(ServiceItem serviceItem);
    void Delete(ServiceItem serviceItem);
    System.Threading.Tasks.Task SaveChangesAsync();
}
