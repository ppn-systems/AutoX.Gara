using System;
using System.Collections.Generic;
using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IServiceItemRepository
{
    System.Threading.Tasks.Task<(List<ServiceItem> Items, int TotalCount)> GetPageAsync(ServiceItemListQuery query);
    System.Threading.Tasks.Task<ServiceItem> GetByIdAsync(int id);
    System.Threading.Tasks.Task AddAsync(ServiceItem serviceItem);
    void Update(ServiceItem serviceItem);
    void Delete(ServiceItem serviceItem);
    System.Threading.Tasks.Task SaveChangesAsync();
}