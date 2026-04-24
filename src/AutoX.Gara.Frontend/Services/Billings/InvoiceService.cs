// Copyright (c) 2026 PPN Corporation. All rights reserved.
using AutoX.Gara.Domain.Enums.Payments;
using AutoX.Gara.Frontend.Models.Results.Billings;
using AutoX.Gara.Contracts.Enums;
using AutoX.Gara.Contracts.Billings;
using Nalix.Common.Networking.Protocols;
using Nalix.Framework.DataFrames.SignalFrames;
using Nalix.Framework.Injection;
using Nalix.SDK.Transport;
using Nalix.SDK.Transport.Extensions;
using System;
namespace AutoX.Gara.Frontend.Services.Billings;
public sealed class InvoiceService
{
    private const int QueryTimeoutMs = 10_000;
    private const int WriteTimeoutMs = 20_000;
    private readonly InvoiceQueryCache _cache;
    public InvoiceService(InvoiceQueryCache cache)
        => _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));
    public async System.Threading.Tasks.Task<InvoiceListResult> GetListAsync(
        int page,
        int pageSize,
        int filterCustomerId,
        string? searchTerm = null,
        InvoiceSortField sortBy = InvoiceSortField.InvoiceDate,
        bool sortDescending = true,
        PaymentStatus? filterPaymentStatus = null,
        DateTime? filterFromDate = null,
        DateTime? filterToDate = null,
        System.Threading.CancellationToken ct = default)
    {
        InvoiceCacheKey key = new(page, pageSize, searchTerm ?? "", sortBy, sortDescending, filterCustomerId, filterPaymentStatus, filterFromDate, filterToDate);
        if (_cache.TryGet(key, out InvoiceCacheEntry? cached))
        {
            return InvoiceListResult.Success(cached!.Invoices, cached!.TotalCount, page * pageSize < cached!.TotalCount);
        }
        try
        {
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            InvoiceQueryRequest packet = new() { Page = page, PageSize = pageSize, SearchTerm = searchTerm ?? "", SortBy = sortBy, SortDescending = sortDescending, FilterCustomerId = filterCustomerId, FilterPaymentStatus = filterPaymentStatus, FilterFromDate = filterFromDate, FilterToDate = filterToDate, OpCode = (System.UInt16)OpCommand.INVOICE_GET };
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(packet, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(QueryTimeoutMs).WithEncrypt(), predicate: p => p is InvoiceQueryResponse or Directive, ct: ct).ConfigureAwait(false);
            if (r is InvoiceQueryResponse resp)
            {
                _cache.Set(key, resp.Invoices, resp.TotalCount);
                return InvoiceListResult.Success(resp.Invoices, resp.TotalCount, page * pageSize < resp.TotalCount);
            }
            return r is Directive err
                ? InvoiceListResult.Failure(err.Reason.ToString(), err.Action)
                : InvoiceListResult.Failure("Unknown response", ProtocolAdvice.NONE);
        }
        catch (Exception ex) { return InvoiceListResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
    public async System.Threading.Tasks.Task<InvoiceWriteResult> CreateAsync(InvoiceDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.INVOICE_CREATE, data, true, ct);
    public async System.Threading.Tasks.Task<InvoiceWriteResult> UpdateAsync(InvoiceDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.INVOICE_UPDATE, data, true, ct);
    public async System.Threading.Tasks.Task<InvoiceWriteResult> DeleteAsync(InvoiceDto data, System.Threading.CancellationToken ct = default) => await SendWriteAsync((System.UInt16)OpCommand.INVOICE_DELETE, data, false, ct);
    private async System.Threading.Tasks.Task<InvoiceWriteResult> SendWriteAsync(System.UInt16 op, InvoiceDto data, bool echo, System.Threading.CancellationToken ct)
    {
        try
        {
            data.OpCode = op;
            TcpSession client = InstanceManager.Instance.GetExistingInstance<TcpSession>()!;
            Nalix.Common.Networking.Packets.IPacket r = await client.RequestAsync<Nalix.Common.Networking.Packets.IPacket>(data, options: Nalix.SDK.Options.RequestOptions.Default.WithTimeout(WriteTimeoutMs).WithEncrypt(), predicate: p => (echo && p is InvoiceDto) || p is Directive, ct: ct).ConfigureAwait(false);
            if (echo && r is InvoiceDto confirmed)
            {
                return InvoiceWriteResult.Success(confirmed);
            }
            if (r is Directive resp)
            {
                if (resp.Type == ControlType.NONE) { _cache.Invalidate(); return InvoiceWriteResult.Success(); }
                return InvoiceWriteResult.Failure(resp.Reason.ToString(), resp.Action);
            }
            return InvoiceWriteResult.Failure("Unknown", ProtocolAdvice.NONE);
        }
        catch (Exception ex) { return InvoiceWriteResult.Failure(ex.Message, ProtocolAdvice.NONE); }
    }
}


