using AutoX.Gara.Domain.Entities.Billings;
using AutoX.Gara.Shared.Models;

namespace AutoX.Gara.Application.Abstractions.Repositories;

public interface IInvoiceRepository
{
    System.Threading.Tasks.Task<Invoice> GetInvoiceWithFullGraphTrackedAsync(System.Int32 id);
    System.Threading.Tasks.Task<(System.Collections.Generic.List<Invoice> Items, System.Int32 TotalCount)> GetPageAsync(InvoiceListQuery query);
    System.Threading.Tasks.Task<Invoice> GetByIdAsync(System.Int32 id);
    System.Threading.Tasks.Task<System.Boolean> ExistsByInvoiceNumberAsync(System.String invoiceNumber, System.Int32? excludeId = null);
    System.Threading.Tasks.Task<Invoice> GetByIdWithDetailsAsync(System.Int32 id);
    System.Threading.Tasks.Task AddAsync(Invoice invoice);
    void Update(Invoice invoice);
    void Delete(Invoice invoice);
    System.Threading.Tasks.Task SaveChangesAsync();
}
