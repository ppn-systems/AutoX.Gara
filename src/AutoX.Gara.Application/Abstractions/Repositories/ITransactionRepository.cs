using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Shared.Models;
using System.Collections.Generic;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface ITransactionRepository
{
    System.Threading.Tasks.Task<(List<Transaction> Items, int TotalCount)> GetPageAsync(TransactionListQuery query);
    System.Threading.Tasks.Task<Transaction> GetByIdAsync(int id);
    System.Threading.Tasks.Task AddAsync(Transaction transaction);
    void Update(Transaction transaction);
    void Delete(Transaction transaction);
    System.Threading.Tasks.Task SaveChangesAsync();
}
