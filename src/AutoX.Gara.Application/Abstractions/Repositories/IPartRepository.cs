using System;
using System.Collections.Generic;
using AutoX.Gara.Domain.Entities.Inventory;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

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