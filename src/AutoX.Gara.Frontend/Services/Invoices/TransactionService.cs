using AutoX.Gara.Shared.Enums;
using System;
// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Domain.Enums.Payments;

using AutoX.Gara.Domain.Enums.Transactions;

using AutoX.Gara.Frontend.Results.Billings;

using Nalix.Common.Networking.Protocols;

using AutoX.Gara.Shared.Protocol.Invoices;

using Microsoft.Extensions.Logging;


using Nalix.Framework.Injection;

using Nalix.Framework.Random;

using Nalix.SDK.Transport;

using Nalix.SDK.Transport.Extensions;

using Nalix.Framework.DataFrames.SignalFrames;

namespace AutoX.Gara.Frontend.Services.Invoices;

public sealed class TransactionService

{
    private const int RequestTimeoutMs = 10_000;

    private readonly TransactionQueryCache _cache;

    public TransactionService(TransactionQueryCache cache)

        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));

    public async System.Threading.Tasks.Task<TransactionListResult> GetListAsync(

        int page,

        int pageSize,

        int filterInvoiceId,

        string? searchTerm = null,

        TransactionSortField sortBy = TransactionSortField.TransactionDate,

        bool sortDescending = true,

        TransactionType? filterType = null,

        TransactionStatus? filterStatus = null,

        PaymentMethod? filterPaymentMethod = null,

        System.Threading.CancellationToken ct = default)

    {
        TransactionCacheKey key = new(page, pageSize, searchTerm ?? "", sortBy, sortDescending, filterInvoiceId, filterType, filterStatus, filterPaymentMethod);

        if (_cache.TryGet(key, out TransactionCacheEntry? cached))

        {
            return TransactionListResult.Success(cached!.Transactions, cached!.TotalCount, page * pageSize < cached!.TotalCount);

        }

        try

        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;

            TransactionQueryRequest packet = new() { Page = page, PageSize = pageSize, SearchTerm = searchTerm ?? "", SortBy = sortBy, SortDescending = sortDescending, FilterInvoiceId = filterInvoiceId, FilterType = filterType, FilterStatus = filterStatus, FilterPaymentMethod = filterPaymentMethod, OpCode = (System.UInt16)OpCommand.TRANSACTION_GET };

            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => p is TransactionQueryResponse or Directive, ct: ct).ConfigureAwait(false);

            if (r is TransactionQueryResponse resp)

            {
                _cache.Set(key, resp.Transactions, resp.TotalCount);

                return TransactionListResult.Success(resp.Transactions, resp.TotalCount, page * pageSize < resp.TotalCount);

            }

            if (r is Directive err) return TransactionListResult.Failure(err.Reason.ToString(), err.Action);

            return TransactionListResult.Failure("Unknown response", ProtocolAdvice.NONE);

        }

        catch (Exception ex) { return TransactionListResult.Failure(ex.Message, ProtocolAdvice.NONE); }

    }

    public async System.Threading.Tasks.Task<TransactionWriteResult> CreateAsync(TransactionDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.TRANSACTION_CREATE, data, true, ct);

    public async System.Threading.Tasks.Task<TransactionWriteResult> UpdateAsync(TransactionDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.TRANSACTION_UPDATE, data, true, ct);

    public async System.Threading.Tasks.Task<TransactionWriteResult> DeleteAsync(TransactionDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.TRANSACTION_DELETE, data, false, ct);

    private async System.Threading.Tasks.Task<TransactionWriteResult> SendWriteAsync(System.UInt16 op, TransactionDto data, bool echo, System.Threading.CancellationToken ct)

    {
        try

        {
            data.OpCode = op;

            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;

            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(RequestTimeoutMs).WithEncrypt(), predicate: p => (echo && p is TransactionDto) || p is Directive, ct: ct).ConfigureAwait(false);

            if (echo && r is TransactionDto confirmed) return TransactionWriteResult.Success(confirmed);

            if (r is Directive resp)

            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(); return TransactionWriteResult.Success(); }

                return TransactionWriteResult.Failure(resp.Reason.ToString(), resp.Action);

            }

            return TransactionWriteResult.Failure("Unknown", ProtocolAdvice.NONE);

        }

        catch (Exception ex) { return TransactionWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }

    }
}
