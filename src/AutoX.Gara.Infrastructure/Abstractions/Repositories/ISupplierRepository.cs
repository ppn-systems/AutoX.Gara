// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Entities.Suppliers;
using AutoX.Gara.Shared.Models;
using System.Collections.Generic;

namespace AutoX.Gara.Infrastructure.Abstractions.Repositories;

/// <summary>
/// �?nh nghia contract cho t?t c? thao t�c dữ liệu li�n quan d?n <see cref="Supplier"/>.
/// <para>
/// Application layer ch? ph? thu?c v�o interface n�y � kh�ng bi?t g� v? EF Core.
/// </para>
/// </summary>
public interface ISupplierRepository
{
    // --- Query ----------------------------------------------------------------

    /// <summary>
    /// L?y danh s�ch nh� cung c?p c� ph�n trang, t�m ki?m, l?c v� s?p x?p.
    /// </summary>
    System.Threading.Tasks.Task<(List<Supplier> Items, int TotalCount)> GetPageAsync(
        SupplierListQuery query,
        System.Threading.CancellationToken ct = default);

    /// <summary>
    /// L?y chi ti?t m?t nh� cung c?p theo ID,
    /// bao g?m navigation property <c>PhoneNumbers</c>.
    /// </summary>
    System.Threading.Tasks.Task<Supplier> GetByIdAsync(
        int id,
        System.Threading.CancellationToken ct = default);

    /// <summary>
    /// Ki?m tra nh� cung c?p d� t?n t?i theo email ho?c m� s? thu?.
    /// D�ng d? tr�nh t?o tr�ng khi Create.
    /// </summary>
    System.Threading.Tasks.Task<bool> ExistsByContactAsync(
        string email,
        string taxCode,
        System.Threading.CancellationToken ct = default);

    // --- Write ----------------------------------------------------------------

    /// <summary>Th�m m?i m?t nh� cung c?p (chua SaveChanges).</summary>
    System.Threading.Tasks.Task AddAsync(
        Supplier supplier,
        System.Threading.CancellationToken ct = default);

    /// <summary>��nh d?u entity l� Modified (chua SaveChanges).</summary>
    void Update(Supplier supplier);

    /// <summary>Luu t?t c? thay d?i v�o database.</summary>
    System.Threading.Tasks.Task SaveChangesAsync(
        System.Threading.CancellationToken ct = default);
}
