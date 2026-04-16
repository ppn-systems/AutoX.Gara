using AutoX.Gara.Domain.Entities.Invoices;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface ITransactionRepository
{
    System.Threading.Tasks.Task<(System.Collections.Generic.List<Transaction> Items, System.Int32 TotalCount)> GetPageAsync(TransactionListQuery query);
    System.Threading.Tasks.Task<Transaction> GetByIdAsync(System.Int32 id);
    System.Threading.Tasks.Task AddAsync(Transaction transaction);
    void Update(Transaction transaction);
    void Delete(Transaction transaction);
    System.Threading.Tasks.Task SaveChangesAsync();
}
