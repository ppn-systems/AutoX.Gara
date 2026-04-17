using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums;

using AutoX.Gara.Domain.Enums.Payments;

using AutoX.Gara.Frontend.Results.Suppliers;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Protocol.Suppliers;

using Microsoft.Extensions.Logging;


using Nalix.Framework.Injection;

using Nalix.Framework.Random;

using Nalix.SDK.Transport;

using Nalix.SDK.Transport.Extensions;

using Nalix.Framework.DataFrames.SignalFrames;

namespace AutoX.Gara.Frontend.Services.Suppliers;

/// <summary>

/// Frontend service for supplier operations.

/// Handles GET/POST/PUT for supplier management.

/// </summary>

public sealed class SupplierService : ISupplierService

{
    private const int RequestTimeoutMs = 10_000;

    private readonly ISupplierQueryCache _cache;

    public SupplierService(ISupplierQueryCache cache) => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<SupplierListResult> GetListAsync(

        int page,

        int pageSize,

        string? searchTerm = null,

        SupplierSortField sortBy = SupplierSortField.Name,

        bool sortDescending = false,

        SupplierStatus filterStatus = SupplierStatus.None,

        PaymentTerms filterPaymentTerms = PaymentTerms.None,

        System.Threading.CancellationToken ct = default)

    {
        SupplierCacheKey key = new(page, pageSize, searchTerm ?? "", sortBy, sortDescending, filterStatus, filterPaymentTerms);

        if (_cache.TryGet(key, out SupplierCacheEntry? cached))

        {
            return SupplierListResult.Success(cached!.Suppliers, cached!.TotalCount, page * pageSize < cached!.TotalCount);

        }

        try

        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;

            SupplierQueryRequest packet = new() { Page = page, PageSize = pageSize, SearchTerm = searchTerm ?? "", SortBy = sortBy, SortDescending = sortDescending, FilterStatus = filterStatus, FilterPaymentTerms = filterPaymentTerms, OpCode = (System.UInt16)OpCommand.SUPPLIER_GET };

            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => p is SupplierQueryResponse or Directive, ct: ct).ConfigureAwait(false);

            if (r is SupplierQueryResponse resp)

            {
                _cache.Set(key, resp.Suppliers, resp.TotalCount);

                return SupplierListResult.Success(resp.Suppliers, resp.TotalCount, page * pageSize < resp.TotalCount);

            }

            if (r is Directive err) return SupplierListResult.Failure(err.Reason.ToString(), err.Action);

            return SupplierListResult.Failure("Unknown response", ProtocolAdvice.NONE);

        }

        catch (System.TimeoutException) { return SupplierListResult.Timeout(); }

        catch (Exception ex) { return SupplierListResult.Failure(ex.Message, ProtocolAdvice.NONE); }

    }

    public async System.Threading.Tasks.Task<SupplierWriteResult> CreateAsync(SupplierDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.SUPPLIER_CREATE, data, true, ct);

    public async System.Threading.Tasks.Task<SupplierWriteResult> UpdateAsync(SupplierDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.SUPPLIER_UPDATE, data, true, ct);

    public async System.Threading.Tasks.Task<SupplierWriteResult> ChangeStatusAsync(SupplierDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.SUPPLIER_CHANGE_STATUS, data, false, ct);

    private async System.Threading.Tasks.Task<SupplierWriteResult> SendWriteAsync(System.UInt16 op, SupplierDto data, bool echo, System.Threading.CancellationToken ct)

    {
        try

        {
            data.OpCode = op;

            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;

            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => (echo && p is SupplierDto) || p is Directive, ct: ct).ConfigureAwait(false);

            if (echo && r is SupplierDto confirmed) return SupplierWriteResult.Success(confirmed);

            if (r is Directive resp)

            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(); return SupplierWriteResult.Success(); }

                return SupplierWriteResult.Failure(resp.Reason.ToString(), resp.Action);

            }

            return SupplierWriteResult.Failure("Unknown", ProtocolAdvice.NONE);

        }

        catch (Exception ex) { return SupplierWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }

    }
}

public interface ISupplierService

{
    System.Threading.Tasks.Task<SupplierListResult> GetListAsync(

        int page,

        int pageSize,

        string? searchTerm = null,

        SupplierSortField sortBy = SupplierSortField.Name,

        bool sortDescending = false,

        SupplierStatus filterStatus = SupplierStatus.None,

        PaymentTerms filterPaymentTerms = PaymentTerms.None,

        System.Threading.CancellationToken ct = default);

    System.Threading.Tasks.Task<SupplierWriteResult> CreateAsync(SupplierDto data, System.Threading.CancellationToken ct = default);

    System.Threading.Tasks.Task<SupplierWriteResult> UpdateAsync(SupplierDto data, System.Threading.CancellationToken ct = default);

    System.Threading.Tasks.Task<SupplierWriteResult> ChangeStatusAsync(SupplierDto data, System.Threading.CancellationToken ct = default);
}
