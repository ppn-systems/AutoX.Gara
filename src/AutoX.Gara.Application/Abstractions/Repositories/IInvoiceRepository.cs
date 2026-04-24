using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Contracts.Models;
using System.Collections.Generic;
namespace AutoX.Gara.Application.Abstractions.Repositories;
public interface IInvoiceRepository
{
    System.Threading.Tasks.Task<Invoice> GetInvoiceWithFullGraphTrackedAsync(int id);
    System.Threading.Tasks.Task<(List<Invoice> Items, int TotalCount)> GetPageAsync(InvoiceListQuery query);
    System.Threading.Tasks.Task<Invoice> GetByIdAsync(int id);
    System.Threading.Tasks.Task<bool> ExistsByInvoiceNumberAsync(string invoiceNumber, int? excludeId = null);
    System.Threading.Tasks.Task<Invoice> GetByIdWithDetailsAsync(int id);
    System.Threading.Tasks.Task AddAsync(Invoice invoice);
    void Update(Invoice invoice);
    void Delete(Invoice invoice);
    System.Threading.Tasks.Task SaveChangesAsync();
}

