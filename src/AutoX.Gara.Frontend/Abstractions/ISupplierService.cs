// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Results.Suppliers;
using AutoX.Gara.Frontend.Services.Suppliers;
using AutoX.Gara.Shared.Enums;
using AutoX.Gara.Shared.Protocol.Suppliers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Frontend.Abstractions;

public interface ISupplierService
{
    Task<SupplierListResult> GetListAsync(System.Int32 page,
        System.Int32 pageSize,
        System.String? searchTerm = null,
        SupplierSortField sortBy = SupplierSortField.Name,
        System.Boolean sortDescending = false,
        SupplierStatus filterStatus = SupplierStatus.None,
        PaymentTerms filterPaymentTerms = PaymentTerms.None,
        System.Threading.CancellationToken ct = default);

    Task<SupplierWriteResult> CreateAsync(SupplierDto data, CancellationToken ct = default);
    Task<SupplierWriteResult> UpdateAsync(SupplierDto data, CancellationToken ct = default);
    Task<SupplierWriteResult> ChangeStatusAsync(System.Int32 supplierId, SupplierStatus newStatus, CancellationToken ct = default);
}

// ISupplierQueryCache.cs
public interface ISupplierQueryCache
{
    System.Boolean TryGet(SupplierCacheKey key, out SupplierCacheEntry? entry);
    void Set(SupplierCacheKey key, List<SupplierDto> suppliers, System.Int32 totalCount);
    void Invalidate();
}