// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Models.Results.Suppliers;
using AutoX.Gara.Frontend.Services.Suppliers;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Suppliers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace AutoX.Gara.Frontend.Abstractions;
public interface ISupplierService
{
    Task<SupplierListResult> GetListAsync(int page,
        int pageSize,
        string? searchTerm = null,
        SupplierSortField sortBy = SupplierSortField.Name,
        bool sortDescending = false,
        SupplierStatus filterStatus = SupplierStatus.None,
        PaymentTerms filterPaymentTerms = PaymentTerms.None,
        System.Threading.CancellationToken ct = default);
    Task<SupplierWriteResult> CreateAsync(SupplierDto data, CancellationToken ct = default);
    Task<SupplierWriteResult> UpdateAsync(SupplierDto data, CancellationToken ct = default);
    Task<SupplierWriteResult> ChangeStatusAsync(int supplierId, SupplierStatus newStatus, CancellationToken ct = default);
}
// ISupplierQueryCache.cs
public interface ISupplierQueryCache
{
    bool TryGet(SupplierCacheKey key, out SupplierCacheEntry? entry);
    void Set(SupplierCacheKey key, List<SupplierDto> suppliers, int totalCount);
    void Invalidate();
}


