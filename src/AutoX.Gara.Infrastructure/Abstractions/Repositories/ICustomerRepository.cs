// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Entities.Customers;
using AutoX.Gara.Contracts.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace AutoX.Gara.Infrastructure.Abstractions.Repositories;
/// <summary>
/// Repository interface cho Customer domain.
/// T�ch bi?t domain/application logic kh?i EF Core chi ti?t.
/// <para>
/// Nguy�n t?c DDD: Application layer ch? g?i interface n�y,
/// kh�ng import Microsoft.EntityFrameworkCore hay AutoXDbContext.
/// </para>
/// </summary>
public interface ICustomerRepository
{
    // --- Query ----------------------------------------------------------------
    /// <summary>
    /// L?y m?t trang kh�ch h�ng v?i filter / sort / ph�n trang.
    /// Tr? v? tuple g?m danh s�ch v� t?ng s? b?n ghi kh?p filter (tru?c ph�n trang).
    /// </summary>
    Task<(List<Customer> Items, int TotalCount)> GetPageAsync(
        CustomerListQuery query,
        CancellationToken ct = default);
    /// <summary>
    /// L?y th�ng tin d?y d? c?a m?t kh�ch h�ng theo Id.
    /// Tr? v? <c>null</c> n?u kh�ng t�m th?y ho?c d� b? soft-delete.
    /// </summary>
    Task<Customer> GetByIdAsync(int id, CancellationToken ct = default);
    /// <summary>
    /// Ki?m tra xem email ho?c s? di?n tho?i d� t?n t?i trong DB chua
    /// (ch? x�t b?n ghi chua b? soft-delete).
    /// </summary>
    Task<bool> ExistsByContactAsync(
        string email,
        string phoneNumber,
        CancellationToken ct = default);
    // --- Write ----------------------------------------------------------------
    /// <summary>Th�m m?i entity v�o DbSet (chua SaveChanges).</summary>
    Task AddAsync(Customer customer, CancellationToken ct = default);
    /// <summary>��nh d?u entity l� Modified (chua SaveChanges).</summary>
    void Update(Customer customer);
    /// <summary>Persist t?t c? thay d?i dang ch? xu?ng DB.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}

